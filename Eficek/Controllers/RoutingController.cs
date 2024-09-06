using Eficek.Models;
using Eficek.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

[Route("route")]
[ApiController]
public class RoutingController(StopsService stopsService, RoutingService routingService) : ControllerBase
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
		Trip? currTrip = null;
		var trips = new List<Trip>();
		 
		// TODO : handle specific cases: waiting should contain only one stop and walking from and to
		
		for (var i = 0; i < edges.Count; i++)
		{
			if (currTrip == null || currTrip.Id != edges[i].Trip.TripId)
			{
				currTrip = new Trip(edges[i].Trip.TripId, edges[i].Trip.TripHeadSign);
				trips.Add(currTrip);
			}

			if (i == 0)
			{
				currTrip.Stops.Add(new Stop(nodes[0].Stop.StopId, nodes[0].Stop.StopName, DateTime.Now, nodes[0].Stop.Coordinate));
			}

			var edgeDestination = edges[i].Node;
			currTrip.Stops.Add(new Stop(edgeDestination.Stop.StopId, edgeDestination.Stop.StopName, DateTime.Now, edgeDestination.Stop.Coordinate));
		}
		
		var connection = new Connection(0, trips);
		
		return Ok(connection);
	}
}