using Eficek.Gtfs;

namespace Eficek.Models;

public class ConnectionNode(string stopId, string stopName, int time, Node.State state)
{
	public string StopId { get; } = stopId;
	public string StopName { get; } = stopName;

	public string Time
	{
		get
		{
			var hours = time / 3600;
			return $"{hours}:{(time - hours * 3600) / 60}:{(time - hours * 3600) / 3600}";
		}
	}

	public Node.State State { get; } = state;
}