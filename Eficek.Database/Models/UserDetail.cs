using Eficek.Database.Entities;

namespace Eficek.Database.Models;

public class UserDetail(UserInternal userInternal)
{
	public int Id { get; set; } = userInternal.Id;
	public string UserName { get; set; } = userInternal.UserName;
	public string Email { get; set; } = userInternal.Email;
	public UserInternal.Role Role { get; set; } = userInternal.UserRole;
}