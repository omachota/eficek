using Eficek.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

[Route("route")]
[ApiController]
public class RoutingController(StopsService stopsService) : ControllerBase
{
	/// <summary>
	/// Toto je test
	/// </summary>
	/// <param name="from">StopGroup id</param>
	/// <param name="to">StopGroup id</param>
	[HttpGet("Search")]
	public IActionResult Search(string from, string to)
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

		return Ok();
	}
}