using Eficek.Gtfs;
using Eficek.Models;
using Eficek.Services;
using Microsoft.AspNetCore.Mvc;
using Stop = Eficek.Models.Stop;
using Trip = Eficek.Models.Trip;

namespace Eficek.Controllers;

[Route("route")]
[ApiController]
public class RoutingController(
	StopsService stopsService,
	RoutingService routingService,
	ILogger<RoutingController> logger) : ControllerBase
{
	/// <summary>
	/// Toto je test
	/// </summary>
	/// <param name="from">StopGroup id</param>
	/// <param name="to">StopGroup id</param>
	/// <param name="start">ISO8601 date</param>
	/// <returns>Not found if either from or to doesn't exist</returns>
	[HttpGet("Search")]
	[ProducesResponseType(typeof(Connection), 200)]
	[ProducesResponseType(typeof(string), 404)]
	public IActionResult Search(string from, string to, DateTime start)
	{
		var fs = stopsService.TryGet(from);
		var ts = stopsService.TryGet(to);

		if (fs == null)
		{
			return NotFound($"StopGroup {from} not found");
		}

		if (ts == null)
		{
			return NotFound($"StopGroup {to} not found");
		}

		// Start search 10 times. Stop if next departure is a day later than start 

		// we can only return the first node plus edges, because edges contain link to destination node
		var (nodes, edges) = routingService.Search(fs, ts, start);

		// TODO : handle specific cases: waiting and walking should contain from and to
		var stops = new List<Stop>();
		var trips = new List<Trip>();
		// This should be written more crearly
		for (var i = 0; i < edges.Count - 1; i++)
		{
			var edge = edges[i];
			if (i == 0)
			{
				stops.Add(
					new Stop(nodes[0].Stop.StopId, nodes[0].Stop.StopName, edge.Node.Time, nodes[0].Stop.Coordinate));
			}

			Edge? nextEdge;
			Node? edgeDestination;
			switch (edge.Trip.Kind)
			{
				case Kind.Connection:
					nextEdge = edges[i + 1];
					edgeDestination = edge.Node;
					if (nextEdge.Trip.Kind == edge.Trip.Kind)
					{
						stops.Add(new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
							edgeDestination.Time, edgeDestination.Stop.Coordinate));
					}
					else if (i < edges.Count - 2)
					{
						var trip = new Trip(edge.Trip.TripId, edge.Trip.Name(), edge.Direction(), stops);
						trips.Add(trip);
						stops =
						[
							new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
								edgeDestination.Time, edgeDestination.Stop.Coordinate)
						];
					}
					else
					{
						stops.Add(new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
							edgeDestination.Time, edgeDestination.Stop.Coordinate));
						var trip = new Trip(edge.Trip.TripId, edge.Trip.Name(), edge.Direction(), stops);
						trips.Add(trip);
						stops = [];
					}

					break;
				case Kind.Walking:
					nextEdge = edges[i + 1];
					// add a stop if Stops are empty or when waiting end == nextEdge is different
					edgeDestination = edge.Node;
					if (stops.Count == 0)
					{
						stops.Add(new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
							edgeDestination.Time, edgeDestination.Stop.Coordinate));
					}

					if (nextEdge.Trip.Kind != edge.Trip.Kind)
					{
						stops.Add(new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
							edgeDestination.Time, edgeDestination.Stop.Coordinate));
						var trip = new Trip(edge.Trip.TripId, edge.Trip.Name(), edge.Direction(), stops);
						trips.Add(trip);
						stops = [];
					}

					break;
				case Kind.Waiting:
					nextEdge = edges[i + 1];
					// add a stop if Stops are empty or when waiting end == nextEdge is different
					edgeDestination = edge.Node;
					if (stops.Count == 0)
					{
						stops.Add(new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
							edgeDestination.Time, edgeDestination.Stop.Coordinate));
					}

					if (nextEdge.Trip.Kind == edge.Trip.Kind)
					{
						continue;
					}

					if (nextEdge.Trip.Kind == Kind.Walking)
					{
						stops.Add(new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
							edgeDestination.Time, edgeDestination.Stop.Coordinate));
						var trip = new Trip(edge.Trip.TripId, edge.Trip.Name(), edge.Direction(), stops);
						trips.Add(trip);
						stops =
						[
							new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
								edgeDestination.Time, edgeDestination.Stop.Coordinate)
						];
					}
					else
					{
						stops.Add(new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
							edgeDestination.Time, edgeDestination.Stop.Coordinate));
						var trip = new Trip(edge.Trip.TripId, edge.Trip.Name(), edge.Direction(), stops);
						trips.Add(trip);
						stops = [];
					}

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		if (edges.Count > 0)
		{
			var last = edges[^1];
			var edgeDestination = last.Node;
			switch (last.Trip.Kind)
			{
				case Kind.Connection:
				case Kind.Walking:
					stops.Add(new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
						edgeDestination.Time, edgeDestination.Stop.Coordinate));
					var trip = new Trip(last.Trip.TripId, last.Trip.Name(), last.Direction(), stops);
					trips.Add(trip);
					break;
				case Kind.Waiting:
					// Should not happen
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		var connection = new Connection(0, trips);

		return Ok(connection);
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