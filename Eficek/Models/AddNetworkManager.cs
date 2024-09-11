using Eficek.Database.Models;

namespace Eficek.Models;

public class AddNetworkManager : ICredentials<User>
{
	public LoginCredentials Credentials { get; set; } = null!;
	public User Value { get; set; } = null!;
}