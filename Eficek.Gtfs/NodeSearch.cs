namespace Eficek.Gtfs;

public static class NodeSearch
{
	public static Node? FirstAfter(List<Node> nodes, int time)
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
	public static int IndexOfFirstAfter(List<Node> nodes, int time)
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