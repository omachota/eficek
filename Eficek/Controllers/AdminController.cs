using Eficek.Database;
using Eficek.Models;
using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

[ApiController]
[Route("Admin")]
#if (!DEBUG)
// This Controller should be hidden from ordinary users
[ApiExplorerSettings(IgnoreApi = true)]
#endif
public class AdminController(DatabaseUserService databaseUserService) : ControllerBase
{
	[HttpPost("Add")]
	[ProducesResponseType(typeof(OkResult), 200)]
	[ProducesResponseType(typeof(UnauthorizedResult), 401)]
	public async Task<IActionResult> AddNetworkManager([FromBody] AddNetworkManager addNetworkManager)
	{
		if (!await databaseUserService.AuthenticateAdmin(addNetworkManager.Credentials))
		{
			return Unauthorized("Failed to authenticate administrator credentials");
		}

		await databaseUserService.AddNew(addNetworkManager.User);

		return Ok();
	}
}