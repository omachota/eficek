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
	[HttpGet("Search")]
	public IActionResult Search(string from, string to, DateTime start)
	{
		var fs = stopsService.TryGet(from);
		var ts = stopsService.TryGet(to);

		if (fs == null)
		{
			return NotFound($"Stop with id {from} not found");
		}

		if (ts == null)
		{
			return NotFound($"Stop with id {to} not found");
		}

		// Start search 10 times. Stop if next departure is a day later than start 

		var nodes = routingService.Search(fs, ts, start);
		return Ok(nodes.Select(node => new ConnectionNode(node.Stop.StopId, stopsService.TryGetStop(node.Stop.StopId)!.StopName, node.Time, node.S)));
	}
}