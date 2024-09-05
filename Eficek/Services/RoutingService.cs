using System.Diagnostics;
using Eficek.Gtfs;

namespace Eficek.Services;

public readonly struct SearchConnectionDuration(int seconds, int dayCompensation)
{
	public readonly int Seconds = seconds;
	public readonly int DayCompensation = dayCompensation;

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
		var times = new int[network.Nodes.Count];
		for (var i = 0; i < times.Length; i++)
		{
			times[i] = int.MaxValue;
		}

		foreach (var stop in from.Stops)
		{
			var node = FirstStopNodeAfter(stop, start.Hour * 60 * 60 + start.Minute * 60 + start.Second);
			if (node == null)
				continue; // Stop doesn't have any nodes 
			queue.Enqueue(node, new SearchConnectionDuration(0, 0)); // priority will be travel time
			times[node.InternalId] = 0;
		}

		var destinationNodeId = -1;
		while (queue.Count > 0)
		{
			if (!queue.TryDequeue(out var node, out var priority))
			{
				throw new UnreachableException();
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
				if (times[internalId] != int.MaxValue || nextNodeTime.Seconds > times[internalId] ||
				    !node.Edges[i].OperatesOn(start.DayOfWeek))
					continue; // ignore if visited earlier
				edges[internalId] = node.Edges[i];
				backTrack[internalId] = node;
				times[internalId] = nextNodeTime.Seconds;
				queue.Enqueue(next, new SearchConnectionDuration(next.Time, 0));
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

	/// <summary>
	/// Find all departures from stopGroup. The number of departures is limited by `maxEntries` and tomorrow 23:59:59.
	/// </summary>
	/// <param name="stopGroup"></param>
	/// <param name="maxEntries"></param>
	/// <returns>Returns sorted departures - from earliest to latest</returns>
	/// <exception cref="NotImplementedException"></exception>
	public List<Node> StopGroupDepartures(StopGroup stopGroup, int maxEntries = 100)
	{
		var tomorrow = DateTime.Today.Add(new TimeSpan(1, 23, 59, 59));
		var results = new List<Node>();
		var now = DateTime.Now;

		var stopNodes = new List<(int, List<Node>)>();
		foreach (var stop in stopGroup.Stops)
		{
			if (!networkService.Network.StopNodes.TryGetValue(stop, out var nodes))
			{
				continue;
			}

			stopNodes.Add((NodeSearch.IndexOfFirstAfter(nodes, now.Hour * 60 + now.Minute + now.Second), nodes));
		}

		var candidates = new List<Edge>();
		var cmp = new EdgeDestinationTimeComparer();
		while (results.Count < maxEntries)
		{
			var count = MinimalCandidates(stopNodes, candidates, cmp);
			if (count == 0)
			{
				break;
			}

			for (var i = 0; i < candidates.Count; i++)
			{
				if (results.Count >= maxEntries)
				{
					goto end;
				}

				// results.Add(candidates[i]);
			}

			candidates.Clear();
		}

		end:

		throw new NotImplementedException();
	}


	private static int MinimalCandidates(List<(int, List<Node>)> stopNodes, List<Edge> candidates, IComparer<Edge> cmp)
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
				if (edges[j].Node.S != Node.State.OnBoard)
				{
					// Skip non boarding edges
					continue;
				}

				candidates.Add(edges[j]);
			}

			// stopNodes[i].Item1 = ++idx;
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