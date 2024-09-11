using System.Text.Json.Serialization;

namespace Eficek.Database.Models;

public class UpdateUserPassword : ICredentials<IdPassword>
{
	public LoginCredentials Credentials { get; set; } = null!;
	public IdPassword Value { get; set; } = null!;
}

public class IdPassword
{
	[JsonIgnore]
	public int Id { get; set; }

	public string Password { get; set; }
}