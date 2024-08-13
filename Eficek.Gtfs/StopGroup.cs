using CoordinateSharp;

namespace Eficek.Gtfs;

/// <summary>
/// Contains all stops with the same group name.
/// </summary>
public class StopGroup
{
	public Dictionary<string, Stop> Stops;
	public Coordinate Coordinate; // Probably mean from others?
}