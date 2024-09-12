using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class Trip(string routeId, string serviceId, string tripId, string tripHeadSign, Kind kind)
	: IFromRow<Trip>
{
	public Trip(string routeId, string tripId, string tripHeadSign, Service service, Kind kind) : this(routeId,
		service.ServiceId, tripId, tripHeadSign, kind)
	{
		Service = service;
	}

	public readonly string RouteId = routeId;
	public readonly string ServiceId = serviceId;
	public readonly string TripId = tripId;
	public readonly string TripHeadSign = tripHeadSign;
	public readonly Kind Kind = kind;
	public Service? Service;
	public Route? Route;
	public int? Delay;


	public readonly List<StopTime> StopTimes = [];
	// TripShortName omitted


	public static Trip FromRow(SepReader.Row row)
	{
		return new Trip(
			row["route_id"].ToString(),
			row["service_id"].ToString(),
			row["trip_id"].ToString(),
			row["trip_headsign"].ToString().Trim('\"'),
			Kind.Connection
		);
	}
}

public enum Kind
{
	Connection,
	Walking,
	Waiting,
}