using Eficek.Gtfs;
using Eficek.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

[Route("stops")]
[ApiController]
public class StopsController(NetworkService networkService, StopsService stopsService) : ControllerBase
{
	[HttpGet("GetAll")]
	public IEnumerable<StopGroup> GetAll()
	{
		return networkService.Network.StopGroups.Values;
	}

	[HttpGet("GetNearby")]
	public IEnumerable<StopGroup> GetNearby(double lat, double lon)
	{
		return stopsService.GetNearby(new Coordinate(lat, lon));
	}
}