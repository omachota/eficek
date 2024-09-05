using System.Diagnostics;
using Eficek.Gtfs;

namespace Eficek.Services;

public class RoutingService(NetworkService networkService, ILogger<RoutingService> logger)
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="from">Valid StopGroup</param>
	/// <param name="to"></param>
	/// <param name="start"></param>
	/// <returns></returns>
	public List<Node> Search(StopGroup from, StopGroup to, DateTime start)
	{
		var network = networkService.Network;
		var queue = new PriorityQueue<Node, int>();

		var backTrack = new Node?[network.Nodes.Count];
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
			queue.Enqueue(node, 0); // priority will be travel time
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
			
			if (node.S == Node.State.InStop && to.Stops.Contains(node.Stop))
			{
				destinationNodeId = node.InternalId;
				break;
			}

			for (var i = 0; i < node.Edges.Count; i++)
			{
				var next = node.Edges[i].Node;
				var internalId = next.InternalId;
				var edgeTime = next.Time - node.Time;
				var nextNodeTime = priority + edgeTime;
				if (times[internalId] != int.MaxValue || nextNodeTime > times[internalId] || !node.Edges[i].OperatesOn(start.DayOfWeek))
					continue; // ignore if visited earlier
				backTrack[internalId] = node;
				times[internalId] = nextNodeTime;
				queue.Enqueue(next, next.Time);
			}
		}


		var connectionNodes = new List<Node>();
		if (destinationNodeId == -1)
			return connectionNodes;
		var current = network.Nodes[destinationNodeId];
		while (current != null)
		{
			connectionNodes.Add(current);
			current = backTrack[current.InternalId];
		}

		connectionNodes.Reverse();
		logger.LogInformation("Connection found");

		return connectionNodes;
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
			if (!networkService.Network.StopNodes.TryGetValue(stop.StopId, out var nodes))
			{
				continue;
			}

			stopNodes.Add((IndexOfFirstAfter(nodes, now.Hour * 60 + now.Minute + now.Second), nodes));
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
		return networkService.Network.StopNodes.TryGetValue(stop.StopId, out var nodes)
			? FirstAfter(nodes, time)
			: null; // This stop does not have any nodes
	}

	private static Node? FirstAfter(List<Node> nodes, int time)
	{
		var idx = IndexOfFirstAfter(nodes, time);
		return idx == -1 ? null : nodes[idx];
	}

	/// <summary>
	/// Find the first Node in `nodes` whose time is greater than `time` in log(nodes).
	/// </summary>
	/// <param name="nodes">Nodes for a stop that are sorted by time</param>
	/// <param name="time">In seconds from the midnight</param>
	/// <returns>Index of the first Node. It returns 0 if a Node is a day later. -1 if no such Node exists.</returns>
	private static int IndexOfFirstAfter(List<Node> nodes, int time)
	{
		var l = 0;
		var r = nodes.Count - 1;
		var closestAt = -1;
		while (l <= r)
		{
			var mid = (l + r) / 2;
			var node = nodes[mid];
			if (node.Time >= time)
			{
				// We always keep a node that has higher time than time
				// closest = node;
				closestAt = mid;
				r = mid - 1;
			}
			else
			{
				l = mid + 1;
			}
		}

		// handle close midnight search
		if (closestAt == -1 && nodes.Count > 0)
		{
			return 0;
		}

		return closestAt;
	}
}