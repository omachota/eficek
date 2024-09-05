namespace Eficek.Models;

public class Trip(string id, string direction)
{
	public string Id { get; } = id;
	public string Direction { get; } = direction;
	public List<Stop> Stops { get; } = [];
}