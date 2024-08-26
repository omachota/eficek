using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class Trip(string routeId, string serviceId, string tripId, string tripHeadSign)
	: IFromRow<Trip>
{
	public string RouteId = routeId;
	public string ServiceId = serviceId;
	public string TripId = tripId;
	public string TripHeadSign = tripHeadSign;

	public List<StopTime> StopTimes = [];
	// TripShortName omitted


	public static Trip FromRow(SepReader.Row row)
	{
		return new Trip(
			row["route_id"].ToString(),
			row["service_id"].ToString(),
			row["trip_id"].ToString(),
			row["trip_headsign"].ToString()
		);
	}
}