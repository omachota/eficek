namespace Eficek.Models;

public class Connection(int duration, List<Trip> trips)
{
	/// <summary>
	/// Represented in seconds
	/// </summary>
	public int Duration { get; } = duration;

	/// <summary>
	/// Trips follow each other and include walking
	/// </summary>
	public List<Trip> Trips { get; } = trips;
}