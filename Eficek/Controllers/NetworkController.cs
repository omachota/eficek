using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

public class Credentials
{
	public string Username { get; }
	public string Password { get; }
}

[Route("admin")]
[ApiController]
public class NetworkController(ILogger<NetworkController> logger)
{
	// Rights - token
	[HttpPost("Update")]
	public async Task Update([FromBody] Credentials credentials)
	{
		logger.LogInformation("{} {}", credentials.Username, credentials.Password);
	}
}