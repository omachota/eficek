using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdministrationController : ControllerBase
{
	[HttpGet("test")]
	public ActionResult<string> Test()
	{
		return "hello";
	}
	
	[HttpGet]
	public ActionResult<string> Tedst()
	{
		return "helldo";
	}
}