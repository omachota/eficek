namespace Eficek.Database.Models;

public class User : PublicUser
{
	public string Password { get; set; } = null!;
}