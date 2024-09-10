namespace Eficek.Gtfs;

public class Edge(Node node, Trip trip, double distance, Edge.EdgeType type)
{
	public readonly Node Node = node;
	public readonly Trip Trip = trip;
	public readonly Service? Service = trip.Service;
	public readonly double Distance = distance;
	public readonly EdgeType Type = type;

	public enum EdgeType
	{
		GetOn,
		GetOff,
		ContinueOnBoard,
		Waiting,
		Walking
	}

	public bool OperatesOn(DayOfWeek day)
	{
		if (Service == null)
		{
			return false;
		}

		return day switch
		{
			DayOfWeek.Sunday => Service.Sunday,
			DayOfWeek.Monday => Service.Monday,
			DayOfWeek.Tuesday => Service.Tuesday,
			DayOfWeek.Wednesday => Service.Wednesday,
			DayOfWeek.Thursday => Service.Thursday,
			DayOfWeek.Friday => Service.Friday,
			DayOfWeek.Saturday => Service.Saturday,
			_ => throw new ArgumentOutOfRangeException(nameof(day), day, null)
		};
	}
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