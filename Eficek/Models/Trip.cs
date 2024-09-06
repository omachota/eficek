namespace Eficek.Models;

public class Trip
{
	public Trip(string id, string direction, List<Stop>? stops = null) : this(id, id, direction, stops)
	{
	}

	public Trip(string id, string name, string direction, List<Stop>? stops = null)
	{
		if (stops != null)
			Stops = stops;
		Id = id;
		Name = name;
		Direction = direction;
	}

	public string Id { get; }
	public string Name { get; }
	public string Direction { get; }
	public List<Stop> Stops { get; } = [];
}