using Eficek.Database.Entities;
using Eficek.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Eficek.Database;

public class DatabaseUserService(EficekDbContext context)
{
	public async Task AddNew(User user)
	{
		var salt = PasswordHasher.Salt();
		var hash = PasswordHasher.Hash(user.Password, salt);

		await context.Users.AddAsync(new UserInternal(UserInternal.Role.NetworkManager, user.Name, user.Email, hash,
			salt));
		await context.SaveChangesAsync();
	}

	public async Task<bool> Authenticate(LoginCredentials credentials)
	{
		var hit = await context.Users.FirstOrDefaultAsync(dbUser =>
			dbUser.UserName == credentials.UserName ||
			dbUser.Email == credentials.UserName);
		if (hit == null)
		{
			return false;
		}

		var hash = PasswordHasher.Hash(credentials.Password, hit.Salt);
		return ValidateHash(hit.Hash, hash);
	}

	public async Task<bool> AuthenticateAdmin(LoginCredentials credentials)
	{
		var hit = await context.Users.FirstOrDefaultAsync(dbUser =>
			dbUser.UserName == credentials.UserName ||
			dbUser.Email == credentials.UserName &&
			dbUser.UserRole == UserInternal.Role.Admin);
		if (hit == null)
		{
			return false;
		}

		var hash = PasswordHasher.Hash(credentials.Password, hit.Salt);
		return ValidateHash(hit.Hash, hash);
	}

	private static bool ValidateHash(byte[] l, byte[] r)
	{
		if (l.Length != r.Length)
		{
			return false;
		}

		for (var i = 0; i < l.Length; i++)
		{
			if (l[i] != r[i])
			{
				return false;
			}
		}

		return true;
	}
}