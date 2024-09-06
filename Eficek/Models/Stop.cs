using Eficek.Gtfs;

namespace Eficek.Models;

public class Stop : IFrom<(DateTime, Gtfs.Stop), Stop>
{
	private int _time;

	public Stop(string id, string name, int time, Coordinate coordinate)
	{
		Id = id;
		Name = name;
		_time = time;
		Coordinate = new CoordinateInfo(coordinate.Latitude, coordinate.Longitude);
	}

	public string Id { get; init; }
	public string Name { get; init; }

	/// <summary>
	/// Time corresponds to departure for the first station. Otherwise, it represents arrival to a station
	/// </summary>
	/// TODO : should be datetime
	public string Time
	{
		get
		{
			var hours = _time / 3600;
			var minutes = (_time - hours * 3600) / 60;
			var seconds = (_time - hours * 3600) / 3600;
			return $"{hours:00}:{minutes:00}:{seconds:00}";
		}
	}

	public CoordinateInfo Coordinate { get; init; }

	// TODO : pass durations
	public static Stop From((DateTime, Gtfs.Stop) from)
	{
		var date = from.Item1;
		var stop = from.Item2;
		return new Stop(stop.StopId, stop.StopName, 0, stop.Coordinate);
	}
}