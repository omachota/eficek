namespace Eficek.Gtfs;

/// <summary>
/// Contains all stops with the same group name.
/// </summary>
public class StopGroup
{
	public Dictionary<string, Stop> Stops = new();
	public Coordinate? Coordinate = null; // Probably mean from others?

	public void AddStop(Stop stop)
	{
		Stops.Add(stop.StopName, stop);
	}
}