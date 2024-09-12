namespace Eficek.Models;

public class Trip
{
	public Trip(string id, string name, string direction, int? delay, List<Stop>? stops = null)
	{
		if (stops != null)
			Stops = stops;
		Id = id;
		Name = name;
		Direction = direction;
		Delay = delay;
	}

	public string Id { get; }
	public string Name { get; }
	public string Direction { get; }
	public int? Delay { get; }
	public List<Stop> Stops { get; } = [];
}