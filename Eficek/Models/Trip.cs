namespace Eficek.Models;

public class Trip(string id, string direction, Stop[] stops)
{
	public string Id { get; } = id;
	public string Direction { get; } = direction;
	public Stop[] Stops { get; } = stops;
}