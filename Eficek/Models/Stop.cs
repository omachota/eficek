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
	public string Time
	{
		get
		{
			var hours = time / 3600;
			var minutes = (time - hours * 3600) / 60;
			var seconds = (time - hours * 3600) / 3600;
			return $"{hours:00}:{minutes:00}:{seconds:00}";
		}
	}

	public Coordinate Coordinate { get; } = coordinate;

	// TODO : pass durations
	public static Stop From((DateTime, Gtfs.Stop) from)
	{
		var date = from.Item1;
		var stop = from.Item2;
		return new Stop(stop.StopId, stop.StopName, 0, stop.Coordinate);
	}
}