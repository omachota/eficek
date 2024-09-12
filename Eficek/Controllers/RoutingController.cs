using System.Diagnostics;
using Eficek.Gtfs;
using Eficek.Infrastructure;
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
	RoutingService routingService) : ControllerBase
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

		var useDelay = start.Date == DateTime.Today.Date;

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
						trips.Add(Trip.From((edge.Trip, edge, stops, useDelay)));
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
						trips.Add(Trip.From((edge.Trip, edge, stops, useDelay)));
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
						trips.Add(Trip.From((edge.Trip, edge, stops, useDelay)));
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
					trips.Add(Trip.From((edge.Trip, edge, stops, useDelay)));
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
					trips.Add(Trip.From((last.Trip, last, stops, useDelay)));
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
		var fromStopGroup = stopsService.TryGet(from);
		var viaStopGroup = stopsService.TryGet(via);
		var toStopGroup = stopsService.TryGet(to);

		if (fromStopGroup == null)
		{
			return NotFound($"StopGroup {from} not found");
		}

		if (viaStopGroup == null)
		{
			return NotFound($"StopGroup {via} not found");
		}

		if (toStopGroup == null)
		{
			return NotFound($"StopGroup {to} not found");
		}

		if (fromStopGroup.GroupId == viaStopGroup.GroupId || viaStopGroup.GroupId == toStopGroup.GroupId)
		{
			return Search(fromStopGroup.GroupId, toStopGroup.GroupId, start);
		}

		var (viaNodes, viaEdges) = routingService.Search(fromStopGroup, viaStopGroup, start);
		if (viaEdges.Count < 1)
		{
			return NotFound("No connection found");
		}

		var viaNode = viaEdges[^1].Node;
		var timeDiff = viaNode.Time - start.SearchTimeInformation().Item1;
		var toResult = routingService.Search(viaNode.Stop, toStopGroup, start.AddSeconds(timeDiff));

		return Ok();
	}

	/// <summary>
	/// Finds all StopGroups that are reachable from `from` StopGroup
	/// </summary>
	/// <param name="from">StopGroup id</param>
	/// <param name="start">ISO 8601 date</param>
	/// <param name="duration">Seconds</param>
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