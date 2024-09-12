using Eficek.Gtfs;

namespace Eficek.Models;

public class Trip : IFrom<(Gtfs.Trip, Edge, List<Stop>?, bool), Trip>
{
	public Trip(string id, string name, string direction, string color, int? delay, List<Stop>? stops = null)
	{
		if (stops != null)
			Stops = stops;
		Id = id;
		Name = name;
		Direction = direction;
		Color = color;
		Delay = delay;
	}

	public string Id { get; }
	public string Name { get; }
	public string Direction { get; }
	public string Color { get; }
	public int? Delay { get; }
	public List<Stop> Stops { get; } = [];

	public static Trip From((Gtfs.Trip, Edge, List<Stop>?, bool) from)
	{
		var (trip, edge, stops, useDelay) = from;
		var routeColor = trip.Route != null ? trip.Route.RouteColor : "000000";
		var delay = useDelay ? trip.Delay : null;
		return new Trip(trip.TripId, trip.Name(), edge.Direction(), routeColor, delay, stops);
	}
}

public static class GtsfInfoExtension
{
	public static string Name(this Gtfs.Trip trip)
	{
		if (trip.Route != null)
		{
			return trip.Route.RouteShortName;
		}

		return trip.Kind == Kind.Walking ? "walking" : "waiting";
	}

	public static string Direction(this Edge edge)
	{
		var trip = edge.Trip;
		if (trip.Kind == Kind.Connection)
		{
			return trip.TripHeadSign;
		}

		return trip.Kind == Kind.Walking ? edge.Node.Stop.StopName : "";
	}
}