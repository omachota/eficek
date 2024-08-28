using Eficek.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

[Route("route")]
[ApiController]
public class RoutingController(StopsService stopsService) : ControllerBase
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="from">StopGroup id</param>
	/// <param name="to">StopGroup id</param>
	[HttpGet("Search")]
	public IActionResult Search(string from, string to)
	{
		var fs = stopsService.TryGet(from);
		var ts = stopsService.TryGet(to);

		if (fs == null || ts == null)
		{
			return NotFound("");
		}

		return Ok();
	}
}