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
				var arr = BuildStopTimeNode(nodes, stop, stopTimes[i].ArrivalTime, Node.State.OnBoard);
				// TODO : Should we check SequenceId?
				previous?.AddEdge(arr, trip); // Connect last dep (now last arr) with arr

				var nextPossibleDeparture = BuildStopTimeNode(nodes, stop,
					stopTimes[i].ArrivalTime + Constants.MinTransferTime, Node.State.InStop);
				arr.AddEdge(nextPossibleDeparture, trip); // Get off the trip
				if (i >= stopTimes.Count - 1)
				{
					break; // This is last stopTime for a trip. No need to create dep node
				}

				var dep = BuildStopTimeNode(nodes, stop, stopTimes[i].DepartureTime, Node.State.InStop);
				dep.AddEdge(arr, trip); // Boarding

				previous = arr;
			}
		}

		return nodes;
	}

	private readonly KeyValueSelector<Stop> _stopSelector = row =>
	{
		var s = Stop.FromRow(row);
		return (s.StopId, s);
	};

	private readonly Func<Stop, bool> _stopFilter = stop => stop.ZoneRegionType is > 0;

	public async Task<Network> BuildAsync(ILogger logger)
	{
		var stopwatch = Stopwatch.StartNew();
		var feedTask = Task.Run(() => ParseSingle<FeedInfo>(BuildRelativeFilePath("feed_info.txt")));
		var stopsTask = Task.Run(() => Parse<Stop>(BuildRelativeFilePath("stops.txt"), _stopFilter));
		var routesTask = Task.Run(() => Parse<Route>(BuildRelativeFilePath("routes.txt")));
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

		try
		{
			await Task.WhenAll(feedTask, stopsTask, routesTask, tripsTask, stopTimesTask);
		}
		catch (Exception e)
		{
			logger.LogError(e, "Failed to parse gtfs data");
			throw;
		}

		UtmCoordinateBuilder.AssignUtmCoordinate(stopsTask.Result);
		var stopGroups = GenerateStopGroups(stopsTask.Result);
		FillTripsWithStopTimes(tripsTask.Result, stopTimesTask.Result);
		var stops = stopsTask.Result.ToFrozenDictionary(stop => stop.StopId);
		var nodes = new ReadOnlyCollection<Node>(BuildStopTimesGraph(tripsTask.Result, stops));

		var network = new Network
		{
			Nodes = nodes,
			Stops = stops,
			StopGroups = stopGroups.ToFrozenDictionary(),
			NearbyStopGroups = AssignStopGroupsToSquares(stopGroups).ToFrozenDictionary(),
			StopNodes = GenerateStopNodes(nodes).ToFrozenDictionary()
		};

		stopwatch.Stop();

		logger.LogInformation(
			"Gtfs data successfully parsed in {} milliseconds\n\tStops: {}\n\tRoutes: {}\n\tTrips: {}\n\tStopTimes: {}",
			stopwatch.ElapsedMilliseconds, stopsTask.Result.Count, routesTask.Result.Count, tripsTask.Result.Count,
			stopTimesTask.Result.Count);

		return network;
	}

	private string BuildRelativeFilePath(string fileName)
	{
		return Path.Combine(path, "Prague", fileName);
	}

	private static readonly Trip _waiting = new Trip("0", "1111111", "waiting", "Čekačka");

	private static Dictionary<string, List<Node>> GenerateStopNodes(IList<Node> nodes)
	{
		var stopNodes = new Dictionary<string, List<Node>>();

		for (var i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].S != Node.State.InStop)
			{
				continue; // Skip nodes from which we can't start
			}

			if (stopNodes.TryGetValue(nodes[i].Stop.StopId, out var list))
			{
				list.Add(nodes[i]);
			}
			else
			{
				stopNodes[nodes[i].Stop.StopId] = [nodes[i]];
			}
		}

		// Waiting edges
		foreach (var (_, n) in stopNodes)
		{
			n.Sort(new TimeNodeComparer());
			for (var i = 0; i < n.Count - 1; i++)
			{
				nodes[n[i].InternalId].AddEdge(nodes[n[i+1].InternalId], _waiting);
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
			// Since the whole Czech republic is in 33U, we don't have to care about UTM rectangles now
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