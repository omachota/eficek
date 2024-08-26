using System.Text;

namespace Eficek.Gtfs;

/// <summary>
/// Contains all stops with the same group name.
/// </summary>
public class StopGroup(string groupName)
{
	public HashSet<Stop> Stops = new();
	public Coordinate? Coordinate; // Probably mean from others?
	public UtmCoordinate UtmCoordinate;
	public string Name { get; private set; }
	public string GroupName { get; } = groupName;

	public void AddStop(Stop stop)
	{
		Stops.Add(stop);
		// Just for now
		Name = stop.StopName;
		Coordinate = stop.Coordinate;
		UtmCoordinate = stop.UtmCoordinate;
	}

	public override string ToString()
	{
		var sb = new StringBuilder();
		foreach (var stop in Stops)
		{
			sb.Append(stop.StopName);
			sb.Append(';');
		}

		return sb.ToString();
	}
}