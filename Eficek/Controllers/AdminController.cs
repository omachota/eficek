using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

[ApiController]
[Route("Admin")]
#if (!DEBUG)
// This Controller should be hidden from ordinary users
[ApiExplorerSettings(IgnoreApi = true)]
#endif
public class AdminController : ControllerBase
{
	[HttpGet("Test")]
	public void Test() {}

	public IActionResult AddNetworkManager()
	{
		throw new NotImplementedException();
	}
}