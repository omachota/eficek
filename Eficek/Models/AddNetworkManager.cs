using Eficek.Database.Models;

namespace Eficek.Models;

public class AddNetworkManager
{
	public LoginCredentials Credentials { get; set; } = null!;
	public User User { get; set; } = null!;
}