using System.Globalization;
using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class StopTime(string tripId, int arrivalTime, int departureTime, string stopId, int stopSequence)
	: IFromRow<StopTime>
{
	public string TripId = tripId;
	public int ArrivalTime = arrivalTime;
	public int DepartureTime = departureTime;
	public string StopId = stopId;
	public int StopSequence = stopSequence;


	public static StopTime FromRow(SepReader.Row row)
	{
		var x = row["arrival_time"].ToString();
		return new StopTime(
			row["trip_id"].ToString(),
			Seconds.Parse(row["arrival_time"].ToString()),
			Seconds.Parse(row["departure_time"].ToString()),
			row["stop_id"].ToString(),
			row["stop_sequence"].Parse<int>()
		);
	}
}

internal static class Seconds
{
	public static int Parse(string timeSpan)
	{
		var delim = timeSpan.IndexOf(':');
		var hours = int.Parse(timeSpan[..delim]);
		delim--; // we add one if hours consist of 2 ciphers, otherwise zero
		var minutes = int.Parse(timeSpan[(2 + delim)..(4 + delim)]);
		var seconds = int.Parse(timeSpan[(5 + delim)..timeSpan.Length]);

		return hours * 3600 + minutes * 60 + seconds;
	}
}