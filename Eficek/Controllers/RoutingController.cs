using System.Diagnostics;
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
	/// Returns connection between `from` StopGroup and `to` StopGroup
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

		/*
		 * IDEA: Start search ~10 times. Stop if next departure is a day later than `start`.
		 * This may be resources consuming. Simultaneous searching might help. And a faster algorithm.
		 */

		// we can only return the first node plus edges, because edges contain link to destination node
		var (nodes, edges) = routingService.Search(fs, ts, start);

		if (nodes.Count == 0)
		{
			return NotFound("No connection found");
		}

		var stops = new List<Stop>();
		var trips = new List<Trip>();
		var endTime = 0;
		var startTime = nodes[0].Time;
		// TODO : make sure that this stop exists
		stops.Add(
			new Stop(nodes[0].Stop.StopId, nodes[0].Stop.StopName, nodes[0].Time, nodes[0].Stop.Coordinate));
		/*
		 * Following code should be written more clearly
		 * IDEA: - Group waiting into two stops that represent the start and the end of waiting
		 *		 - Walking can be expressed as a trip with two stops. The head-sign of the trip should be set to
		 *		   destination stop name.
		 */
		for (var i = 0; i < edges.Count - 1; i++)
		{
			var edge = edges[i];

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
						endTime = edgeDestination.Time;
						var trip = new Trip(edge.Trip.TripId, edge.Trip.Name(), edge.Direction(), edge.Trip.Delay,
							stops);
						trips.Add(trip);
						stops =
						[
							new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
								edgeDestination.Time, edgeDestination.Stop.Coordinate)
						];
					}
					else
					{
						endTime = edgeDestination.Time;
						stops.Add(new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
							edgeDestination.Time, edgeDestination.Stop.Coordinate));
						var trip = new Trip(edge.Trip.TripId, edge.Trip.Name(), edge.Direction(), edge.Trip.Delay,
							stops);
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
						endTime = edgeDestination.Time;
						stops.Add(new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
							edgeDestination.Time, edgeDestination.Stop.Coordinate));
						var trip = new Trip(edge.Trip.TripId, edge.Trip.Name(), edge.Direction(), edge.Trip.Delay,
							stops);
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

					endTime = edgeDestination.Time;
					stops.Add(new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
						edgeDestination.Time, edgeDestination.Stop.Coordinate));
					var tr = new Trip(edge.Trip.TripId, edge.Trip.Name(), edge.Direction(), edge.Trip.Delay, stops);
					trips.Add(tr);
					if (nextEdge.Trip.Kind == Kind.Walking)
					{
						stops =
						[
							new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
								edgeDestination.Time, edgeDestination.Stop.Coordinate)
						];
					}
					else
					{
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
					endTime = edgeDestination.Time;
					stops.Add(new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName,
						edgeDestination.Time, edgeDestination.Stop.Coordinate));
					var trip = new Trip(last.Trip.TripId, last.Trip.Name(), last.Direction(), last.Trip.Delay, stops);
					trips.Add(trip);
					break;
				case Kind.Waiting:
					// Should not happen
					throw new UnreachableException();
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		var connection = new Connection(endTime - startTime, trips);

		return Ok(connection);
	}

	[HttpGet("SearchVia")]
	public IActionResult SearchVia(string from, string via, string to, DateTime start)
	{
		var fs = stopsService.TryGet(from);
		var vs = stopsService.TryGet(via);
		var ts = stopsService.TryGet(to);

		if (fs == null)
		{
			return NotFound($"StopGroup {from} not found");
		}

		if (vs == null)
		{
			return NotFound($"StopGroup {to} not found");
		}

		if (ts == null)
		{
			return NotFound($"StopGroup {to} not found");
		}

		return Ok();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="from">StopGroup id</param>
	/// <param name="start">ISO 8601 date</param>
	/// <param name="duration">Seconds</param>
	/// <returns></returns>
	[HttpGet("Coverage")]
	public IActionResult Coverage(string from, DateTime start, int duration)
	{
		var fs = stopsService.TryGet(from);

		if (fs == null)
		{
			return NotFound($"StopGroup {from} not found");
		}

		var coverage = routingService.Coverage(fs, start, duration);
		var seenStops = new HashSet<string>();
		var stopArrivals = new List<CoverageResult>();

		for (var i = 0; i < coverage.Count; i++)
		{
			var stopId = coverage[i].Item1.Stop.GroupName();
			if (seenStops.Add(stopId))
			{
				stopArrivals.Add(CoverageResult.From(coverage[i]));
			}
		}

		return Ok(stopArrivals);
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