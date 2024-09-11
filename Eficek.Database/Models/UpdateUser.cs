namespace Eficek.Database.Models;

public class UpdateUser : ICredentials<IdUser>
{
	public LoginCredentials Credentials { get; set; } = null!;
	public IdUser Value { get; set; } = null!;
}