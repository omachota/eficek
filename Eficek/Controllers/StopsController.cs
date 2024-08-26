using Eficek.Gtfs;
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
	public IActionResult GetNearby(double lat, double lon)
	{
		var b = Ok(stopsService.GetNearby(new Coordinate(lat, lon)));
		return b;
	}

	[HttpGet("Search")]
	public IEnumerable<string> Search(string value)
	{
		if (value.Length < 3)
			return Array.Empty<string>();

		throw new NotImplementedException();
	}
}