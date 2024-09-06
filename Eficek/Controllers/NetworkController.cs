using Eficek.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

public class Credentials
{
	public string Username { get; set; }
	public string Password { get; set; }
}

[Route("admin")]
[ApiController]
public class NetworkController(NetworkSingletonService networkSingletonService, ILogger<NetworkController> logger)
{
	// Rights - token
	[HttpPost("Update")]
	public async Task Update([FromBody] Credentials credentials)
	{
		logger.LogInformation("{} {}", credentials.Username, credentials.Password);
		var gtfsCoreDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

		await networkSingletonService.Update(gtfsCoreDirectory);
	}
}