namespace Eficek.Gtfs;

public class Edge(Node node, Trip trip)
{
	public readonly Node Node = node;
	public readonly Trip Trip = trip;
}

public class EdgeDestinationTimeComparer : IComparer<Edge>
{
	public int Compare(Edge? x, Edge? y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (ReferenceEquals(null, y)) return 1;
		if (ReferenceEquals(null, x)) return -1;
		return x.Node.Time.CompareTo(y.Node.Time);
	}
}