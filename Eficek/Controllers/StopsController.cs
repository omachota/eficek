using Eficek.Gtfs;
using Eficek.Models;
using Eficek.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

[Route("stops")]
[ApiController]
public class StopsController(NetworkService networkService, StopsService stopsService) : ControllerBase
{
	[Obsolete("We should not allow user to get all StopGroups unless we change the amount of data")]
	[HttpGet("GetAll")]
	public IEnumerable<StopGroup> GetAll()
	{
		return networkService.Network.StopGroups.Values;
	}

	[HttpGet("GetNearby")]
	public IEnumerable<NearbyStopGroup> GetNearby(double lat, double lon)
	{
		return stopsService.GetNearby(new Coordinate(lat, lon));
	}

	[HttpGet("Search")]
	public IEnumerable<StopGroupMatch> Search(string value)
	{
		return value.Length < 3 ? Array.Empty<StopGroupMatch>() : stopsService.Search(value, -1);
	}
}