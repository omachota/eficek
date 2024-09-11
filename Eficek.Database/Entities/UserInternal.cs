namespace Eficek.Database.Entities;

public class UserInternal()
{
	public UserInternal(Role role, string userName, string email, byte[] hash, byte[] salt) : this()
	{
		UserRole = role;
		UserName = userName;
		Email = email;
		Hash = hash;
		Salt = salt;
	}

	public enum Role
	{
		Admin,
		NetworkManager
	}

	public int Id { get; init; }
	public Role UserRole { get; set; }
	public string UserName { get; set; } = null!;
	public string Email { get; set; } = null!;
	public byte[] Hash { get; set; } = null!;
	public byte[] Salt { get; set; } = null!;
}