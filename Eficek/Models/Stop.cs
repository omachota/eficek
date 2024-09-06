using Eficek.Gtfs;

namespace Eficek.Models;

public class Stop(string id, string name, int time, Coordinate coordinate) : IFrom<(DateTime, Gtfs.Stop), Stop>
{
	public string Id { get; } = id;
	public string Name { get; } = name;

	/// <summary>
	/// Time corresponds to departure for the first station. Otherwise, it represents arrival to a station
	/// </summary>
	/// TODO : should be datetime
	public int Time { get; } = time;

	public Coordinate Coordinate { get; } = coordinate;
	// TODO : pass durations
	public static Stop From((DateTime, Gtfs.Stop) from)
	{
		var date = from.Item1;
		var stop = from.Item2;
		return new Stop(stop.StopId, stop.StopName, 0, stop.Coordinate);
	}
}