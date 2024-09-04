namespace Eficek.Gtfs;

public class Edge(Node node, Trip trip)
{
	public readonly Node Node = node;
	public readonly Trip Trip = trip;
}