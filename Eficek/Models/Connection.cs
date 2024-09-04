namespace Eficek.Models;

public class Connection(Trip[] trips, int duration)
{
	/// <summary>
	/// Represented in seconds
	/// </summary>
	public int Duration { get; } = duration;

	/// <summary>
	/// Trips follow each other and include walking
	/// </summary>
	public Trip[] Trips { get; } = trips;
}