namespace Eficek.Gtfs;

public class Node(int id, string stopId, DateTime time, Node.State s)
{
	public enum State
	{
		InStop,
		
	}

	public readonly int Id = id;
	public readonly string StopId = stopId;
	public readonly DateTime Time = time;
	public readonly State S = s;
	public List<Node> Edges = []; // Should be ImmutableList
}