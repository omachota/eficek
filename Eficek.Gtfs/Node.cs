namespace Eficek.Gtfs;

public class Node(int internalId, Stop stop, int time, Node.State s)
{
	public enum State
	{
		ArrivedInStop,
		DepartingFromStop,
		OnBoard,
	}

	public readonly int InternalId = internalId;

	public readonly Stop Stop = stop;
	// From Midnight in seconds
	public readonly int Time = time;
	public readonly State S = s;
	public readonly List<Edge> Edges = []; // Should be ImmutableList

	public void AddEdge(Node to, Trip trip)
	{
		// Console.WriteLine($"Connecting: {Stop.StopName} {Time} {S} with {to.Stop.StopName} {to.Time} {to.S}");
		Edges.Add(new Edge(to, trip));
	}
}

public class TimeNodeComparer : IComparer<Node>
{
	public int Compare(Node? x, Node? y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (ReferenceEquals(null, y)) return 1;
		if (ReferenceEquals(null, x)) return -1;
		return x.Time.CompareTo(y.Time);
	}
}