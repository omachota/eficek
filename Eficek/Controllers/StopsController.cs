using Eficek.Gtfs;
using Eficek.Models;
using Eficek.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

[Route("stops")]
[ApiController]
public class StopsController(NetworkService networkService, StopsService stopsService, RoutingService routingService) : ControllerBase
{
	[Obsolete("We should not allow user to get all StopGroups unless we change the amount of data")]
	[HttpGet("GetAll")]
	public IEnumerable<StopGroup> GetAll()
	{
		return networkService.Network.StopGroups.Values;
	}

	[HttpGet("GetNearby")]
	[ProducesResponseType(typeof(List<NearbyStopGroup>), 200)]

	public IEnumerable<NearbyStopGroup> GetNearby(double lat, double lon)
	{
		return stopsService.GetNearby(new Coordinate(lat, lon));
	}

	[HttpGet("Search")]
	[ProducesResponseType(typeof(List<StopGroupMatch>), 200)]
	public IEnumerable<StopGroupMatch> Search(string value)
	{
		return value.Length < 3 ? Array.Empty<StopGroupMatch>() : stopsService.Search(value, -1);
	}

	[HttpGet("Departures")]
	[ProducesResponseType(typeof(List<Node>), 200)]
	[ProducesResponseType(typeof(string), 404)]

	public IActionResult Departures(string stopId) // or stopGroupId?
	{
		// TODO : convert stopId to stopGroupId
		var stopGroup = stopsService.TryGet(stopId);
		if (stopGroup == null)
		{
			return NotFound($"Stop {stopId} not found");
		}

		return Ok(routingService.StopGroupDepartures(stopGroup));
	}
}