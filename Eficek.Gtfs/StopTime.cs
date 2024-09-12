using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class StopTime(
	string tripId,
	int arrivalTime,
	int departureTime,
	string stopId,
	int stopSequence,
	double travelledDistance)
	: IFromRow<StopTime>
{
	public readonly string TripId = tripId;
	public readonly int ArrivalTime = arrivalTime;
	public readonly int DepartureTime = departureTime;
	public readonly string StopId = stopId;
	public readonly int StopSequence = stopSequence;
	public readonly double TravelledDistance = travelledDistance;


	public static StopTime FromRow(SepReader.Row row)
	{
		return new StopTime(
			row["trip_id"].ToString(),
			Seconds.Parse(row["arrival_time"].ToString()),
			Seconds.Parse(row["departure_time"].ToString()),
			row["stop_id"].ToString(),
			row["stop_sequence"].Parse<int>(),
			row["shape_dist_traveled"].Parse<double>() * 1000
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