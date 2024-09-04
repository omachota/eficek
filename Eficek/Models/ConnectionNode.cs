using Eficek.Gtfs;

namespace Eficek.Models;

public class ConnectionNode(string stopId, string stopName, int time, Node.State state)
{
	public string StopId { get; } = stopId;
	public string StopName { get; } = stopName;
	public int Time { get; } = time;
	public Node.State State { get; } = state;
}