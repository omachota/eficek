using System.Diagnostics;
using Eficek.Gtfs;
using Eficek.Infrastructure;

namespace Eficek.Services;

public struct SearchConnectionDuration(int seconds, int dayCompensation)
{
	public SearchConnectionDuration(int seconds, int dayCompensation, int boardings, double travelledDistance) : this(
		seconds, dayCompensation)
	{
		Boardings = boardings;
		TravelledDistance = travelledDistance;
	}

	public readonly int Seconds = seconds;
	public readonly int DayCompensation = dayCompensation;
	public int Boardings = 0;
	public readonly double TravelledDistance = 0;

	public SearchConnectionDuration AddSeconds(int seconds)
	{
		return new SearchConnectionDuration(Seconds + seconds, DayCompensation);
	}

	public int ToSeconds()
	{
		return Seconds + DayCompensation * 24 * 60 * 60;
	}
}

public class SearchConnectionDurationComparer : IComparer<SearchConnectionDuration>
{
	public int Compare(SearchConnectionDuration x, SearchConnectionDuration y)
	{
		var cmp = x.ToSeconds().CompareTo(y.ToSeconds());

		return cmp == 0 ? x.TravelledDistance.CompareTo(y.TravelledDistance) : cmp;
	}
}

public class RoutingService(NetworkService networkService, ILogger<RoutingService> logger)
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="from">Valid StopGroup</param>
	/// <param name="to"></param>
	/// <param name="start"></param>
	/// <returns></returns>
	public (List<Node>, List<Edge>) Search(StopGroup from, StopGroup to, DateTime start)
	{
		var network = networkService.Network;
		var queue = new PriorityQueue<Node, SearchConnectionDuration>(new SearchConnectionDurationComparer());

		var backTrack = new Node?[network.Nodes.Count];
		var edgesTrack = new Edge?[network.Nodes.Count];
		var timeDistance = new int[network.Nodes.Count];
		// var boardings = new int[network.Nodes.Count];
		var distance = new double[network.Nodes.Count];

		for (var i = 0; i < timeDistance.Length; i++)
		{
			timeDistance[i] = int.MaxValue;
			// boardings[i] = int.MaxValue;
			distance[i] = int.MaxValue;
		}

		MarkStartNodes(from, start, queue, timeDistance);

		var destinationNodeId = -1;
		while (queue.Count > 0)
		{
			if (!queue.TryDequeue(out var node, out var priority))
			{
				throw new UnreachableException();
			}

			// logger.LogInformation("stop: {}, stop_time: {}, time: {}, priority: {}", node.Stop.StopId, node.Time, timeDistance[node.InternalId], priority.ToSeconds());
			// if (timeDistance[node.InternalId] != priority.ToSeconds() &&
			//     boardings[node.InternalId] != priority.Boardings)
			if (timeDistance[node.InternalId] != priority.ToSeconds() &&
			    distance[node.InternalId] < priority.TravelledDistance)
			{
				continue;
			}

			// logger.LogInformation("Processing: {}, {}, {}", node.Stop.StopName, node.Time, node.S);

			if (node.S != Node.State.OnBoard && to.Stops.Contains(node.Stop))
			{
				logger.LogInformation("Connection found with {} boardings", priority.Boardings);
				destinationNodeId = node.InternalId;
				break;
			}

			for (var i = 0; i < node.Edges.Count; i++)
			{
				var next = node.Edges[i].Node;
				var internalId = next.InternalId;
				// if (node.Edges[i].Trip.TripId.StartsWith("375"))
				// {
				// 	logger.LogInformation("trying: {}, {} {} {} {}", node.Edges[i].Trip.TripId, node.Stop.StopId,
				// 		node.Time, next.Stop.StopId, next.Time);
				// }

				// over midnight
				if (next.Time < node.Time)
				{
					continue;
				}

				var edgeTime = next.Time - node.Time;
				var totalDistance = priority.TravelledDistance + node.Edges[i].Distance;
				var nextNodePriority =
					new SearchConnectionDuration(priority.Seconds + edgeTime, 0, priority.Boardings, totalDistance);
				if (node.S == Node.State.DepartingFromStop && next.S == Node.State.OnBoard)
				{
					// increment priority to minimize number of boardings
					nextNodePriority.Boardings++;
				}

				if (((nextNodePriority.Seconds >= timeDistance[internalId] &&
				      nextNodePriority.TravelledDistance >= distance[internalId]) ||
				     nextNodePriority.Seconds > timeDistance[internalId]) || !node.Edges[i].OperatesOn(start.DayOfWeek))
				{
					continue;
				}

				edgesTrack[internalId] = node.Edges[i];
				backTrack[internalId] = node;
				timeDistance[internalId] = nextNodePriority.ToSeconds();
				// boardings[internalId] = nextNodePriority.Boardings;
				distance[internalId] = totalDistance;
				queue.Enqueue(next, nextNodePriority);
			}
		}
		
		var connectionNodes = new List<Node>();
		var takenEdges = new List<Edge>();
		if (destinationNodeId == -1)
			return (connectionNodes, takenEdges);
		var current = network.Nodes[destinationNodeId];
		while (current != null)
		{
			connectionNodes.Add(current);
			var e = edgesTrack[current.InternalId];
			if (e != null)
			{
				takenEdges.Add(e);
			}

			current = backTrack[current.InternalId];
		}

		takenEdges.Reverse();
		connectionNodes.Reverse();
		logger.LogInformation("Connection found with distance: {}", distance[destinationNodeId]);

		return (connectionNodes, takenEdges);
	}

	// Destination, Duration
	public List<(Node, int)> Coverage(StopGroup from, DateTime start, int duration)
	{
		var network = networkService.Network;
		var queue = new PriorityQueue<Node, SearchConnectionDuration>(new SearchConnectionDurationComparer());
		var end = start.Hour * 3600 + start.Minute * 60 + start.Second + duration;

		var timeDistance = new int[network.Nodes.Count];
		var distance = new double[network.Nodes.Count];

		for (var i = 0; i < timeDistance.Length; i++)
		{
			timeDistance[i] = int.MaxValue;
			distance[i] = int.MaxValue;
		}

		MarkStartNodes(from, start, queue, timeDistance);

		var marked = new List<(Node, int)>();
		while (queue.Count > 0)
		{
			if (!queue.TryDequeue(out var node, out var priority))
			{
				throw new UnreachableException();
			}

			// Drop suboptimal
			if (timeDistance[node.InternalId] != priority.ToSeconds() &&
			    distance[node.InternalId] < priority.TravelledDistance)
			{
				continue;
			}

			marked.Add((node, timeDistance[node.InternalId]));

			for (var i = 0; i < node.Edges.Count; i++)
			{
				var edge = node.Edges[i];
				var next = edge.Node;
				var internalId = next.InternalId;
				// over midnight
				if (next.Time < node.Time || node.Time > end)
				{
					continue;
				}

				var edgeTime = next.Time - node.Time;
				var totalDistance = priority.TravelledDistance + edge.Distance;
				var nextNodePriority =
					new SearchConnectionDuration(priority.Seconds + edgeTime, 0, priority.Boardings, totalDistance);
				if (edge.Type == Edge.EdgeType.GetOn)
				{
					// increment priority to minimize number of boardings
					nextNodePriority.Boardings++;
				}

				if ((nextNodePriority.Seconds >= timeDistance[internalId] &&
				     nextNodePriority.TravelledDistance >= distance[internalId]) ||
				    nextNodePriority.Seconds > timeDistance[internalId] || !node.Edges[i].OperatesOn(start.DayOfWeek))
				{
					continue;
				}

				timeDistance[internalId] = nextNodePriority.ToSeconds();
				distance[internalId] = totalDistance;
				queue.Enqueue(next, nextNodePriority);
			}
		}

		return marked;
	}

	private void MarkStartNodes(StopGroup from, DateTime start, PriorityQueue<Node, SearchConnectionDuration> queue,
	                            int[] timeDistance)
	{
		foreach (var stop in from.Stops)
		{
			var node = FirstStopNodeAfter(stop, start.Hour * 60 * 60 + start.Minute * 60 + start.Second);
			if (node == null)
				continue;
			// logger.LogInformation("StartNode: {} {}", node.Stop.StopId, node.Time);
			queue.Enqueue(node, new SearchConnectionDuration(node.Time, 0)); // priority will be travel time
			timeDistance[node.InternalId] = node.Time;
		}
	}


	/// <summary>
	/// Find all departures from stopGroup. The number of departures is limited by `maxEntries` and tomorrow 23:59:59.
	/// </summary>
	/// <param name="stopGroup"></param>
	/// <param name="maxEntries"></param>
	/// <returns>Returns sorted departures - from earliest to latest</returns>
	public List<(Node, Edge, int)> StopGroupDepartures(StopGroup stopGroup, int maxEntries = 100)
	{
		var now = DateTime.Now;
		now = new DateTime(now.Year, now.Month, now.Day, 14, 25, 0);
		var secondsNow = now.Hour * 3600 + now.Minute * 60 + now.Second;
		var day = now.DayOfWeek;

		logger.LogInformation("stops: {}", stopGroup.Stops.Count);
		// We need Node + Edge => Node for departure time and Edge for Trip, int for day compensation
		var candidates = new List<(Node, Edge, int)>();
		foreach (var stop in stopGroup.Stops)
		{
			var dayCompensation = 0;
			if (!networkService.Network.StopNodes.TryGetValue(stop, out var nodes))
			{
				continue;
			}

			var idx = NodeSearch.IndexOfFirstAfter(nodes, secondsNow);
			if (idx == -1 && nodes.Count > 0)
			{
				// TODO : increment day
				day = day.NextDay();
				++dayCompensation;
			}

			if (idx == 0 && nodes.Count > 0)
			{
				// TODO : should not be skipped
				continue;
			}

			// iterate over all nodes in stop from time and for each node over each edge to detect departures
			var dayIterations = 2 - dayCompensation;
			for (var d = 0; d < dayIterations; d++)
			{
				for (var i = idx; i < nodes.Count; i++)
				{
					for (var j = 0; j < nodes[i].Edges.Count; j++)
					{
						var edge = nodes[i].Edges[j];
						// TODO : DayOfWeek might be changed above
						if (edge.Type == Edge.EdgeType.GetOn && edge.OperatesOn(day))
						{
							candidates.Add((nodes[i], edge, dayCompensation));
						}
					}
				}

				idx = 0;
				day = day.NextDay();
				++dayCompensation;
			}

			// logger.LogInformation("idx: {}, nodes: {}, time: {}, stop: {}", idx, nodes.Count, secondsNow, stop.StopId);
			// logger.LogInformation("idx node time: {}", nodes[idx].Time);
		}

		return candidates.OrderBy(x => x.Item3).ThenBy(x => x.Item1.Time).Take(maxEntries).ToList();
	}


	private Node? FirstStopNodeAfter(Stop stop, int time)
	{
		return networkService.Network.StopNodes.TryGetValue(stop, out var nodes)
			? NodeSearch.FirstAfter(nodes, time)
			: null; // This stop does not have any nodes
	}
}