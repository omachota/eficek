using Eficek.Gtfs;

namespace Eficek.Models;

public class Stop(string id, string name, DateTime time, Coordinate coordinate)
{
	public string Id { get; } = id;
	public string Name { get; } = name;

	/// <summary>
	/// Time corresponds to departure for the first station. Otherwise, it represents arrival to a station
	/// </summary>
	public DateTime Time { get; } = time;

	public Coordinate Coordinate { get; } = coordinate;
}