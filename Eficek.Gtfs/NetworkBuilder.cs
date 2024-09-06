using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class NetworkBuilder(string path)
{
	private static List<T> Parse<T>(string filePath, Func<T, bool>? predicate = null) where T : IFromRow<T>
	{
		using var reader = Sep.New(',').Reader().FromFile(filePath);
		var data = new List<T>();

		foreach (var row in reader)
		{
			var value = T.FromRow(row);
			if (predicate != null && !predicate(value))
			{
				continue;
			}

			data.Add(value);
		}

		return data;
	}

	private delegate (string, T) KeyValueSelector<T>(SepReader.Row row);

	private static Dictionary<string, T> ParseDict<T>(string filePath, KeyValueSelector<T> keyValueSelector,
	                                                  Func<T, bool>? predicate = null)
	{
		using var reader = Sep.New(',').Reader().FromFile(filePath);
		var data = new Dictionary<string, T>();

		foreach (var row in reader)
		{
			var (key, value) = keyValueSelector(row);
			if (predicate != null && !predicate(value))
			{
				continue;
			}

			data.Add(key, value);
		}

		return data;
	}

	private static T ParseSingle<T>(string filePath) where T : IFromRow<T>
	{
		using var reader = Sep.New(',').Reader().FromFile(filePath);

		if (reader.MoveNext())
		{
			return T.FromRow(reader.Current);
		}

		// TODO : better exception
		throw new Exception($"File `{filePath}` does not contain data");
	}

	private static void FillTripsWithStopTimes(Dictionary<string, Trip> trips, IReadOnlyList<StopTime> stopTimes)
	{
		// stopTimes are also sorted according to trips 
		Trip? lastTrip = null;
		for (var i = 0; i < stopTimes.Count; i++)
		{
			if (lastTrip != null && stopTimes[i].TripId == lastTrip.TripId)
			{
				lastTrip.StopTimes.Add(stopTimes[i]);
			}
			else
			{
				lastTrip = trips[stopTimes[i].TripId];
				lastTrip.StopTimes.Add(stopTimes[i]);
			}
		}
	}

	private static Node BuildStopTimeNode(List<Node> nodes, Stop stop, int time, Node.State state)
	{
		var node = new Node(nodes.Count, stop, time, state);
		nodes.Add(node);
		return node;
	}

	private static List<Node> BuildStopTimesGraph(IDictionary<string, Trip> trips, IDictionary<string, Stop> stopLookup)
	{
		var nodes = new List<Node>();
		Node? previous = null;
		foreach (var (_, trip) in trips)
		{
			var stopTimes = trip.StopTimes;
			for (var i = 0; i < stopTimes.Count; i++)
			{
				var stop = stopLookup[stopTimes[i].StopId];
				// TODO : ArrivalTime and DepartureTime differ

				var departure = BuildStopTimeNode(nodes, stop, stopTimes[i].DepartureTime, Node.State.OnBoard);
				// TODO : Should we check SequenceId?
				// previous not null, this is at least second stop, connect with previous and create get off edge.
				if (previous != null)
				{
					previous.AddEdge(departure, trip); // Connect last dep (now last arr) with arr
					var getOff = BuildStopTimeNode(nodes, stop, stopTimes[i].ArrivalTime + Constants.MinTransferTime,
						Node.State.ArrivedInStop);
					departure.AddEdge(getOff, trip); // Get off the trip					
				}

				if (i >= stopTimes.Count - 1)
				{
					break; // This is last stopTime for a trip. No need to create dep node
				}

				var getOn = BuildStopTimeNode(nodes, stop, stopTimes[i].DepartureTime, Node.State.DepartingFromStop);
				getOn.AddEdge(departure, trip); // Boarding

				// connect the first boarding with following stop if previous == null, otherwise use departure
				previous = departure;
			}

			previous = null; // This is so important...
		}

		return nodes;
	}

	private readonly Func<Stop, bool> _stopFilter = stop => stop.ZoneRegionType is > 0;

	public async Task<Network> BuildAsync(ILogger logger)
	{
		logger.LogInformation("Started parsing gtfs data");
		var stopwatch = Stopwatch.StartNew();
		var feedTask = Task.Run(() => ParseSingle<FeedInfo>(BuildRelativeFilePath("feed_info.txt")));
		var stopsTask = Task.Run(() => Parse(BuildRelativeFilePath("stops.txt"), _stopFilter));
		var routesTask = Task.Run(() => ParseDict<Route>(BuildRelativeFilePath("routes.txt"), row =>
		{
			var route = Route.FromRow(row);
			return (route.RouteId, route);
		}));
		var tripsTask = Task.Run(() => ParseDict<Trip>(BuildRelativeFilePath("trips.txt"), row =>
		{
			var t = Trip.FromRow(row);
			return (t.TripId, t);
		}));
		var stopTimesTask = Task.Run(() => Parse<StopTime>(BuildRelativeFilePath("stop_times.txt"), stopTime =>
		{
			// Trains contain non stop points
			return stopTime.StopId.StartsWith('U');
		}));
		var calendarTask = Task.Run(() => ParseDict<Service>(BuildRelativeFilePath("calendar.txt"), row =>
		{
			var service = Service.FromRow(row);
			return (service.ServiceId, service);
		}));

		try
		{
			await Task.WhenAll(feedTask, stopsTask, routesTask, tripsTask, stopTimesTask, calendarTask);
		}
		catch (Exception e)
		{
			logger.LogError(e, "Failed to parse gtfs data");
			throw;
		}

		UtmCoordinateBuilder.AssignUtmCoordinate(stopsTask.Result);
		var stopGroups = GenerateStopGroups(stopsTask.Result);
		ConnectServicesWithTrips(tripsTask.Result, calendarTask.Result);
		FillTripsWithStopTimes(tripsTask.Result, stopTimesTask.Result);
		var stops = stopsTask.Result.ToFrozenDictionary(stop => stop.StopId);
		var nodes = new ReadOnlyCollection<Node>(BuildStopTimesGraph(tripsTask.Result, stops));
		var stopNodes = BuildAndConnectStopNodes(nodes);
		AddPedestrianEdges(stopNodes, CalculateNearbyStops(stopsTask.Result));
		AssignRoutesToTrips(routesTask.Result, tripsTask.Result);

		var network = new Network
		{
			Nodes = nodes,
			Stops = stops,
			StopGroups = stopGroups.ToFrozenDictionary(),
			NearbyStopGroups = AssignStopGroupsToSquares(stopGroups).ToFrozenDictionary(),
			StopNodes = stopNodes.ToFrozenDictionary()
		};

		stopwatch.Stop();

		logger.LogInformation(
			"Gtfs data successfully parsed in {} milliseconds\n\tStops: {}\n\tRoutes: {}\n\tTrips: {}\n\tStopTimes: {}\n\tNodes: {}",
			stopwatch.ElapsedMilliseconds, stopsTask.Result.Count, routesTask.Result.Count, tripsTask.Result.Count,
			stopTimesTask.Result.Count, nodes.Count);

		return network;
	}

	private static void AssignRoutesToTrips(Dictionary<string, Route> routes, Dictionary<string,Trip> trips)
	{
		foreach (var (_, trip) in trips)
		{
			trip.Route = routes[trip.RouteId];
		}
	}

	private Dictionary<(int, int), List<Stop>> CalculateNearbyStops(List<Stop> stops)
	{
		var dict = new Dictionary<(int, int), List<Stop>>();
		for (var i = 0; i < stops.Count; i++)
		{
			// Since the nearly whole Czech republic is in 33U, we don't have to care about UTM rectangles now
			var box = stops[i].UtmCoordinate.GetUtmBox();
			if (dict.TryGetValue(box, out var nearbyStopGroups))
			{
				nearbyStopGroups.Add(stops[i]);
			}
			else
			{
				dict[box] = [stops[i]];
			}
		}

		return dict;
	}

	private static void ConnectServicesWithTrips(IDictionary<string, Trip> trips, IDictionary<string, Service> services)
	{
		foreach (var (_, trip) in trips)
		{
			trip.Service = services[trip.ServiceId];
		}
	}

	private struct PedestrianConnectionToStop(Stop stop, int duration)
	{
		/// <summary>
		/// Connection destination
		/// </summary>
		public readonly Stop Stop = stop;

		/// <summary>
		/// Duration in seconds
		/// </summary>
		public readonly int Duration = duration;
	}

	private static List<PedestrianConnectionToStop> GetNearbyStopsWithWalkDuration(
		Stop stop, IDictionary<(int, int), List<Stop>> boxes)
	{
		var utm = stop.UtmCoordinate;
		var (eBox, nBox) = utm.GetUtmBox();

		var nearby = new List<PedestrianConnectionToStop>();

		for (var i = 0; i < Constants.Neighbours.Length; i++)
		{
			// check neighbour box
			if (!boxes.TryGetValue(
				    (eBox + Constants.Neighbours[i].Item1, nBox + Constants.Neighbours[i].Item2),
				    out var candidates))
			{
				continue;
			}

			// iterate over stopGroups in neighbour box
			for (var j = 0; j < candidates.Count; j++)
			{
				var distance = candidates[j].UtmCoordinate.Manhattan(utm);
				if (distance <= Constants.MaxStopWalkDistance && candidates[j].StopId != stop.StopId)
				{
					nearby.Add(new PedestrianConnectionToStop(candidates[j], (int)(distance / Constants.WalkingSpeed)));
				}
			}
		}

		return nearby;
	}

	private static readonly Trip _pedestrianConnection = new("Chůze", "PED", "Pěšky", Service.AllDays("1111111-walk"), Kind.Walking);

	private static void AddPedestrianEdges(IDictionary<Stop, List<Node>> stopNodes,
	                                       IDictionary<(int, int), List<Stop>> boxes)
	{
		foreach (var (stop, nodes) in stopNodes)
		{
			var nearby = GetNearbyStopsWithWalkDuration(stop, boxes);
			for (var i = 0; i < nearby.Count; i++)
			{
				for (var j = 0; j < nodes.Count; j++)
				{
					if (nodes[j].S != Node.State.ArrivedInStop)
					{
						continue;
					}
					if (!stopNodes.TryGetValue(nearby[i].Stop, out var destinationCandidates))
					{
						continue;
					}
					
					var dest = NodeSearch.FirstAfter(destinationCandidates, nodes[j].Time + nearby[i].Duration);
					if (dest == null)
					{
						// Should we ignore it?
						continue;
					}
					
					// Console.WriteLine($"Connecting {nodes[j].Stop.StopId} {nodes[j].Time} {nodes[j].S} with {dest.Stop.StopId} {dest.Time} {dest.S}");

					nodes[j].AddEdge(dest, _pedestrianConnection);
				}
			}
		}
	}

	private string BuildRelativeFilePath(string fileName)
	{
		return Path.Combine(path, "Prague", fileName);
	}

	private static readonly Trip _waiting = new("0", "waiting", "Čekačka", Service.AllDays("1111111-wait"), Kind.Waiting);

	private static Dictionary<Stop, List<Node>> BuildAndConnectStopNodes(IList<Node> nodes)
	{
		var stopNodes = new Dictionary<Stop, List<Node>>();

		for (var i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].S == Node.State.OnBoard)
			{
				continue; // Skip non-stop nodes - they should not be connected with other stop nodes
			}

			if (stopNodes.TryGetValue(nodes[i].Stop, out var list))
			{
				list.Add(nodes[i]);
			}
			else
			{
				stopNodes[nodes[i].Stop] = [nodes[i]];
			}
		}

		foreach (var (_, n) in stopNodes)
		{
			n.Sort(new TimeNodeComparer());
			for (var i = 0; i < n.Count - 1; i++)
			{
				n[i].AddEdge(n[i + 1], _waiting);
				// nodes[n[i].InternalId].AddEdge(nodes[n[i + 1].InternalId], _waiting);
			}
		}

		return stopNodes;
	}

	public Dictionary<string, StopGroup> GenerateStopGroups(IReadOnlyList<Stop> stops)
	{
		var dict = new Dictionary<string, StopGroup>();
		for (var i = 0; i < stops.Count; i++)
		{
			// Todo : GroupName should be precomputed when creating a stop?
			var groupName = stops[i].GroupName();
			if (dict.TryGetValue(groupName, out var stopGroup))
			{
				stopGroup.AddStop(stops[i]);
			}
			else
			{
				var st = new StopGroup(groupName);
				st.AddStop(stops[i]);
				dict[groupName] = st;
			}
		}

		return dict;
	}

	private Dictionary<(int, int), List<StopGroup>> AssignStopGroupsToSquares(
		IReadOnlyDictionary<string, StopGroup> stopGroups)
	{
		var dict = new Dictionary<(int, int), List<StopGroup>>();
		foreach (var (_, stopGroup) in stopGroups)
		{
			// Since the nearly whole Czech republic is in 33U, we don't have to care about UTM rectangles now
			var box = stopGroup.UtmCoordinate.GetUtmBox();
			if (dict.TryGetValue(box, out var nearbyStopGroups))
			{
				nearbyStopGroups.Add(stopGroup);
			}
			else
			{
				dict[box] = [stopGroup];
			}
		}

		return dict;
	}
}