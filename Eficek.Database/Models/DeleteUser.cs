namespace Eficek.Database.Models;

public class DeleteUser : ICredentials<int>
{
	public LoginCredentials Credentials { get; set; } = null!;
	public int Value { get; set; }
}