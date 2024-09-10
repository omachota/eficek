namespace Eficek.Database.Entities;

public class UserInternal(UserInternal.Role role, string userName, string email, byte[] hash, byte[] salt)
{
	public enum Role
	{
		Admin,
		NetworkManager
	}
	
	public int Id { get; init; }
	public Role UserRole { get; set; } = role;
	public string UserName { get; set; } = userName;
	public string Email { get; set; } = email;
	public byte[] Hash { get; set; } = hash;
	public byte[] Salt { get; set; } = salt;
}