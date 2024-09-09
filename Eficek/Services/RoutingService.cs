using System.Diagnostics;
using Eficek.Gtfs;
using Eficek.Infrastructure;

namespace Eficek.Services;

public readonly struct SearchConnectionDuration(int seconds, int dayCompensation)
{
	public readonly int Seconds = seconds;
	public readonly int DayCompensation = dayCompensation;

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
		return x.ToSeconds().CompareTo(y.ToSeconds());
	}
}

public struct SearchPriority
{
	public int Time;
	public int BoardingsCount;
}

public class TimeWithBoardingCountComparer : IComparer<SearchPriority>
{
	public int Compare(SearchPriority x, SearchPriority y)
	{
		var timeComparison = x.Time.CompareTo(y.Time);
		if (timeComparison != 0) return timeComparison;
		return x.BoardingsCount.CompareTo(y.BoardingsCount);
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
		var edges = new Edge?[network.Nodes.Count];
		var timeDistance = new int[network.Nodes.Count];
		for (var i = 0; i < timeDistance.Length; i++)
		{
			timeDistance[i] = int.MaxValue;
		}

		foreach (var stop in from.Stops)
		{
			var node = FirstStopNodeAfter(stop, start.Hour * 60 * 60 + start.Minute * 60 + start.Second);
			if (node == null)
				continue; // Stop doesn't have any nodes 
			queue.Enqueue(node, new SearchConnectionDuration(0, 0)); // priority will be travel time
			timeDistance[node.InternalId] = 0;
		}

		var destinationNodeId = -1;
		while (queue.Count > 0)
		{
			if (!queue.TryDequeue(out var node, out var priority))
			{
				throw new UnreachableException();
			}
			
			// logger.LogInformation("stop: {}, stop_time: {}, time: {}, priority: {}", node.Stop.StopId, node.Time, timeDistance[node.InternalId], priority.ToSeconds());
			if (timeDistance[node.InternalId] != priority.ToSeconds())
			{
				continue;
			}

			// logger.LogInformation("Processing: {}, {}, {}", node.Stop.StopName, node.Time, node.S);

			if (node.S != Node.State.OnBoard && to.Stops.Contains(node.Stop))
			{
				destinationNodeId = node.InternalId;
				break;
			}

			for (var i = 0; i < node.Edges.Count; i++)
			{
				var next = node.Edges[i].Node;
				var internalId = next.InternalId;
				// over midnight
				if (next.Time < node.Time)
				{
					continue;
				}

				var edgeTime = next.Time - node.Time;
				var nextNodeTime = new SearchConnectionDuration(priority.Seconds + edgeTime, 0);
				if (nextNodeTime.Seconds >= timeDistance[internalId] || !node.Edges[i].OperatesOn(start.DayOfWeek))
					continue; // ignore if visited earlier
				edges[internalId] = node.Edges[i];
				backTrack[internalId] = node;
				timeDistance[internalId] = nextNodeTime.ToSeconds();
				queue.Enqueue(next, nextNodeTime);
				//queue.Enqueue(next, new SearchConnectionDuration(next.Time, 0));
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
			var e = edges[current.InternalId];
			if (e != null)
			{
				takenEdges.Add(e);
			}

			current = backTrack[current.InternalId];
		}

		takenEdges.Reverse();
		connectionNodes.Reverse();
		logger.LogInformation("Connection found");

		return (connectionNodes, takenEdges);
	}

	private class IndexToStopNodes(int index, List<Node> nodes)
	{
		public int Index = index;
		public readonly List<Node> Nodes = nodes;

		public void Deconstruct(out int idx, out List<Node> nodes)
		{
			idx = Index;
			nodes = Nodes;
		}
	}

	/// <summary>
	/// Find all departures from stopGroup. The number of departures is limited by `maxEntries` and tomorrow 23:59:59.
	/// </summary>
	/// <param name="stopGroup"></param>
	/// <param name="maxEntries"></param>
	/// <returns>Returns sorted departures - from earliest to latest</returns>
	/// <exception cref="NotImplementedException"></exception>
	public List<(Node, Edge, int)> StopGroupDepartures(StopGroup stopGroup, int maxEntries = 100)
	{
		var now = DateTime.Now;
		var secondsNow = now.Hour * 3600 + now.Minute * 60 + now.Second;
		var day = now.DayOfWeek;
		var dayCompensation = 0;

		logger.LogInformation("stops: {}", stopGroup.Stops.Count);
		// We need Node + Edge => Node for departure time and Edge for Trip, int for day compensation
		var candidates = new List<(Node, Edge, int)>();
		foreach (var stop in stopGroup.Stops)
		{
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
						if (nodes[i].S == Node.State.DepartingFromStop && edge.Node.S == Node.State.OnBoard &&
						    edge.OperatesOn(day))
						{
							candidates.Add((nodes[i], edge, dayCompensation));
						}
					}
				}
				
				idx = 0;
				day = day.NextDay();
				++dayCompensation;
			}


			logger.LogInformation("idx: {}, nodes: {}, time: {}, stop: {}", idx, nodes.Count, secondsNow, stop.StopId);
			logger.LogInformation("idx node time: {}", nodes[idx].Time);
		}

		return candidates.OrderBy(x => x.Item3).ThenBy(x => x.Item1.Time).Take(maxEntries).ToList();

		// var candidates = new List<Edge>();
		// var cmp = new EdgeDestinationTimeComparer();
		// while (results.Count < maxEntries)
		// {
		// 	// No candidates => hit end
		// 	var count = MinimalCandidates(stopNodes, candidates, cmp);
		// 	logger.LogInformation("mincandidates: {}", count);
		// 	if (count == 0)
		// 	{
		// 		break;
		// 	}
		//
		// 	for (var i = 0; i < candidates.Count; i++)
		// 	{
		// 		if (results.Count >= maxEntries)
		// 		{
		// 			goto end;
		// 		}
		//
		// 		results.Add(candidates[i]);
		// 	}
		//
		// 	candidates.Clear();
		// }
	}


	private static int MinimalCandidates(List<IndexToStopNodes> stopNodes, List<Edge> candidates, IComparer<Edge> cmp)
	{
		// TODO : change List<> to LinkedList<>
		for (var i = 0; i < stopNodes.Count; i++)
		{
			var (idx, nodes) = stopNodes[i];
			if (idx >= nodes.Count)
			{
				continue;
			}

			var edges = nodes[idx].Edges;
			for (var j = 0; j < edges.Count; j++)
			{
				// Consider only departures from a stop
				// TODO : trip cantakeedge
				if (edges[j].Node.S == Node.State.OnBoard && nodes[idx].S == Node.State.DepartingFromStop)
				{
					candidates.Add(edges[j]);
				}
			}

			++stopNodes[i].Index;
		}

		candidates.Sort(cmp);

		return candidates.Count;
	}

	private Node? FirstStopNodeAfter(Stop stop, int time)
	{
		return networkService.Network.StopNodes.TryGetValue(stop, out var nodes)
			? NodeSearch.FirstAfter(nodes, time)
			: null; // This stop does not have any nodes
	}
}