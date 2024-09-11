using Eficek.Database.Entities;
using Eficek.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Eficek.Database;


public class DatabaseUserService(EficekDbContext context)
{
	/// <summary>
	/// Add new Network Manager into the database
	/// A programmer is responsible for authentication
	/// </summary>
	public async Task AddNew(User user)
	{
		var salt = PasswordHasher.Salt();
		var hash = PasswordHasher.Hash(user.Password, salt);

		await context.Users.AddAsync(new UserInternal(UserInternal.Role.NetworkManager, user.UserName, user.Email, hash,
			salt));
		await context.SaveChangesAsync();
	}

	/// <summary>
	/// Get id of a user given UserName or Email. If there isn't such user, -1 is returned
	/// </summary>
	public async Task<int> GetId(string userNameOrEmail)
	{
		var user = await context.Users.FirstOrDefaultAsync(user => user.UserName == userNameOrEmail ||
		                                                           user.Email == userNameOrEmail);
		if (user == null)
		{
			return -1;
		}

		return user.Id;
	}

	/// <summary>
	///	Get all users from the database with ids
	/// A programmer is responsible for authentication
	/// </summary>
	public async Task<List<UserDetail>> GetAll()
	{
		return await context.Users.Select(user => new UserDetail(user)).ToListAsync();
	}

	/// <summary>
	/// Update UserName or Email of a user. For updating password, use UpdatePassword
	/// </summary>
	/// <returns>True if changes were saved. False if user with `user.Id` was not found</returns>
	public async Task<bool> Update(IdUser user)
	{
		var dbUser = await context.Users.FirstOrDefaultAsync(dbUser => dbUser.Id == user.Id);
		if (dbUser == null)
		{
			return false;
		}

		dbUser.UserName = user.UserName;
		dbUser.Email = user.Email;
		await context.SaveChangesAsync();
		return true;
	}

	/// <summary>
	///  Update a password of a user with given id
	/// </summary>
	public async Task<bool> UpdatePassword(IdPassword idPassword)
	{
		var user = await context.Users.FirstOrDefaultAsync(user => user.Id == idPassword.Id);
		if (user == null)
		{
			return false;
		}

		var salt = PasswordHasher.Salt();
		user.Hash = PasswordHasher.Hash(idPassword.Password, salt);
		user.Salt = salt;
		await context.SaveChangesAsync();
		return true;
	}

	/// <summary>
	/// Deletes a user with given id. Returns True if the user was deleted 
	/// </summary>
	public async Task<bool> Delete(int id)
	{
		var user = await context.Users.FirstOrDefaultAsync(user => user.Id == id);
		if (user == null)
		{
			return false;
		}

		context.Users.Remove(user);
		var res = await context.SaveChangesAsync();
		return res == 1;
	}

	/// <summary>
	/// Authenticate user or admin
	/// </summary>
	/// <param name="credentials">User or admin credentials. UserName can contain UserName or Email</param>
	/// <returns>True if authentication was successful, otherwise false</returns>
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
		return PasswordHasher.ValidateHash(hit.Hash, hash);
	}

	/// <summary>
	/// Authenticate admin
	/// </summary>
	/// <param name="credentials">Admin credentials. UserName can contain UserName or Email</param>
	/// <returns>True if authentication was successful, otherwise false</returns>
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
		return PasswordHasher.ValidateHash(hit.Hash, hash);
	}
}