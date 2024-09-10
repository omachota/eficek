using Eficek.Gtfs;
using Eficek.Models;
using Eficek.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

[Route("stops")]
[ApiController]
public class StopsController(NetworkService networkService, StopsService stopsService, RoutingService routingService)
	: ControllerBase
{
	[Obsolete("We should not allow user to get all StopGroups unless we change the amount of data")]
	[HttpGet("GetAll")]
	public IEnumerable<StopGroup> GetAll()
	{
		return networkService.Network.StopGroups.Values;
	}

	/// <summary>
	/// Returns StopGroups that are near given `latitude` and `longitude`
	/// </summary>
	/// <param name="lat">Latitude between -90 and 90</param>
	/// <param name="lon">Longitude between -180 and 180</param>
	/// <returns></returns>
	[HttpGet("GetNearby")]
	[ProducesResponseType(typeof(List<NearbyStopGroup>), 200)]
	public IEnumerable<NearbyStopGroup> GetNearby(double lat, double lon)
	{
		return stopsService.GetNearby(new Coordinate(lat, lon));
	}

	/// <summary>
	/// Search StopGroups by StopGroup name
	/// </summary>
	/// <param name="value">If the length of value is less than 3, empty response is returned</param>
	/// <returns></returns>
	[HttpGet("Search")]
	[ProducesResponseType(typeof(List<StopGroupMatch>), 200)]
	public IEnumerable<StopGroupMatch> Search(string value)
	{
		return value.Length < 3 ? Array.Empty<StopGroupMatch>() : stopsService.Search(value, -1);
	}

	/// <summary>
	/// Returns a set of departures from a StopGroup
	/// </summary>
	/// <param name="stopId"></param>
	/// <returns>A set of DepartureResult. 404 is returned if stopGroup doesn't exist</returns>
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

		return Ok(routingService.StopGroupDepartures(stopGroup)
		                        .Select(x => new DepartureResult(x.Item2.Trip.TripId, x.Item1.Time)));
	}
}