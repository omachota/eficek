using System.Security.Cryptography;
using System.Text;
using Eficek.Database;
using Eficek.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

public class Credentials
{
	public string Username { get; set; } = null!;
	public string Password { get; set; } = null!;
}

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
	public async ValueTask<IActionResult> Update([FromBody] Credentials credentials)
	{
		var sb = new StringBuilder();
		var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(credentials.Password));
		for (var i = 0; i < bytes.Length; i++)
		{
			sb.Append(bytes[i].ToString("x2"));
		}

		var hash = sb.ToString();
		logger.LogInformation("{}", hash);
		// admin:admin for now
		if (credentials.Username != "admin" ||
		    hash != "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918")
		{
			return Unauthorized();
		}

		logger.LogInformation("User {} logged in successfully and requested API update", credentials.Username);
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