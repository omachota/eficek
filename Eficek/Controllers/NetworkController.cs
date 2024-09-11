using Eficek.Database;
using Eficek.Database.Models;
using Eficek.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

[Route("Network")]
[ApiController]
public class NetworkController(
	NetworkSingletonService networkSingletonService,
	DatabaseUserService databaseUserService,
	ILogger<NetworkController> logger) : ControllerBase
{
	/// <summary>
	/// This endpoint allows to download new gtfs data and update search graph without shutting down the API.
	/// It the API is being updated, 
	/// </summary>
	/// <param name="credentials">Your secret username and password</param>
	[HttpPost("Update")]
	[ProducesResponseType(typeof(OkResult), 200)]
	[ProducesResponseType(typeof(UnauthorizedResult), 401)]
	public async ValueTask<IActionResult> Update([FromBody] LoginCredentials credentials)
	{
		if (!await databaseUserService.Authenticate(credentials))
		{
			return Unauthorized("Failed to authenticate user");
		}

		logger.LogInformation("User {} logged in successfully and requested API update", credentials.UserName);
		var gtfsCoreDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

		// should we await this call?
		await networkSingletonService.Update(gtfsCoreDirectory);
		return Ok();
	}

	/// <summary>
	/// Show current update status
	/// </summary>
	/// <returns>True if API is being updated, otherwise false</returns>
	[HttpGet("IsBeingUpdated")]
	[ProducesResponseType(typeof(bool), 200)]
	public bool IsBeingUpdate()
	{
		return networkSingletonService.IsBeingUpdated;
	}
}