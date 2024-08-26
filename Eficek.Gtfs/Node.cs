namespace Eficek.Gtfs;

public class Node(string stopId, int time, Node.State s)
{
	public enum State
	{
		InStop,
		OnBoard,
	}

	public readonly string StopId = stopId;
	// From Midnight
	public readonly int Time = time;
	public readonly State S = s;
	public List<Node> Edges = []; // Should be ImmutableList

	public void AddEdge(Node to)
	{
		Edges.Add(to);
	}
}