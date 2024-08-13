using CoordinateSharp;
using Eficek.Gtfs;
using Eficek.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

[Route("stops")]
[ApiController]
public class StopsController(NetworkService networkService) : ControllerBase
{
	private NetworkService _networkService = networkService;

	public IEnumerable<StopGroup> GetAll()
	{
		
		throw new NotImplementedException();
	}

	public IEnumerable<StopGroup> GetNearby(double lat, double lon)
	{
		var coordinate = new Coordinate(lat, lon);
		var nearby = new List<StopGroup>();
		
		
		return nearby;
	}
}