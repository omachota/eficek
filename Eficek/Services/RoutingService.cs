using System.Diagnostics;
using Eficek.Gtfs;

namespace Eficek.Services;

public class RoutingService(NetworkService networkService)
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="from">Valid StopGroup</param>
	/// <param name="to"></param>
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
			var node = FirstStopNodeAfter(stop, 0);
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
			
			if (to.Stops.Contains(node.Stop))
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
				if (nextNodeTime > times[internalId])
					continue; // ignore if visited earlier
				backTrack[internalId] = node;
				times[internalId] = nextNodeTime;
				queue.Enqueue(next, nextNodeTime);
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

		return connectionNodes;
	}

	private Node? FirstStopNodeAfter(Stop stop, int time)
	{
		return networkService.Network.StopNodes.TryGetValue(stop.StopId, out var nodes)
			? FirstAfter(nodes, time)
			: null; // This stop does not have any nodes
	}

	// nodes are sorted by time
	private static Node? FirstAfter(List<Node> nodes, int time)
	{
		var l = 0;
		var r = nodes.Count - 1;
		Node? closest = null;
		while (l <= r)
		{
			var mid = (l + r) / 2;
			var node = nodes[mid];
			if (node.Time >= time)
			{
				// We always keep a node that has higher time than time
				closest = node;
				r = mid - 1;
			}
			else // (node.Time < time)
			{
				l = mid + 1;
			}
			
		}

		// handle close midnight search
		if (closest == null && nodes.Count > 0)
		{
			return nodes[0];
		}

		return closest;
	}
}