using Eficek.Database;
using Eficek.Database.Models;
using Eficek.Models;
using Microsoft.AspNetCore.Mvc;

namespace Eficek.Controllers;

[ApiController]
[Route("Admin")]
#if (!DEBUG)
// This Controller should be hidden from ordinary users on RELEASE
[ApiExplorerSettings(IgnoreApi = true)]
#endif
public class AdminController(DatabaseUserService databaseUserService) : ControllerBase
{
	private const string AdminAuthFailedMessage = "Failed to authenticate user";

	/// <summary>
	/// 
	/// </summary>
	/// <param name="addNetworkManager">AdminCredentials with new user details</param>
	/// <returns></returns>
	[HttpPost("Add")]
	[ProducesResponseType(typeof(OkResult), 200)]
	[ProducesResponseType(typeof(string), 401)]
	public async Task<IActionResult> AddNetworkManager([FromBody] AddNetworkManager addNetworkManager)
	{
		return await GenericCall(databaseUserService.AuthenticateAdmin, databaseUserService.AddNew, addNetworkManager);
	}

	/// <summary>
	/// Return all user from the database, salts and hashes are omitted
	/// </summary>
	/// <param name="credentials">Admin credentials</param>
	[HttpGet("GetAll")]
	[ProducesResponseType(typeof(List<UserDetail>), 200)]
	[ProducesResponseType(typeof(string), 401)]
	public async Task<IActionResult> GetAll([FromBody] LoginCredentials credentials)
	{
		if (!await databaseUserService.AuthenticateAdmin(credentials))
		{
			return Unauthorized(AdminAuthFailedMessage);
		}

		return Ok(await databaseUserService.GetAll());
	}

	/// <summary>
	/// Update information about users. 
	/// </summary>
	/// <param name="updateUser">Administrator credentials with IdUser - Id</param>
	/// <returns></returns>
	[HttpPost("Update")]
	[ProducesResponseType(typeof(bool), 200)]
	[ProducesResponseType(typeof(string), 401)]
	public async Task<IActionResult> Update([FromBody] UpdateUser updateUser)
	{
		return await GenericCall(databaseUserService.AuthenticateAdmin, databaseUserService.Update, updateUser);
	}

	/// <summary>
	/// Updates password for a user if the given credentials are valid
	/// </summary>
	/// <param name="updateUserPassword">Users credentials and a new password</param>
	/// <returns>True if the password was updated</returns>
	[HttpPost("UpdatePassword")]
	public async Task<IActionResult> UpdatePassword(UpdateUserPassword updateUserPassword)
	{
		// Get userId from database, then authenticate
		var userName = updateUserPassword.Credentials.UserName;
		updateUserPassword.Value.Id = await databaseUserService.GetId(userName);
		return await GenericCall(databaseUserService.Authenticate, databaseUserService.UpdatePassword,
			updateUserPassword);
	}

	/// <summary>
	/// Delete user by given id
	/// </summary>
	/// <param name="deleteUser">Administrator credentials with id of a user to be deleted</param>
	/// <returns>True if the user was deleted successfully</returns>
	[HttpPost("Delete")]
	[ProducesResponseType(typeof(bool), 200)]
	[ProducesResponseType(typeof(string), 401)]
	public async Task<IActionResult> Delete([FromBody] DeleteUser deleteUser)
	{
		return await GenericCall(databaseUserService.AuthenticateAdmin, databaseUserService.Delete, deleteUser);
	}

	private async Task<IActionResult> GenericCall<TIn, TOut>(Authorizer auth,
	                                                         DatabaseUserServiceFunction<TIn, TOut> f,
	                                                         ICredentials<TIn> input)
	{
		if (!await auth(input.Credentials))
		{
			return Unauthorized(AdminAuthFailedMessage);
		}

		return Ok(await f(input.Value));
	}
}

internal delegate Task<TOut> DatabaseUserServiceFunction<in TIn, TOut>(TIn input);

internal delegate Task<bool> Authorizer(LoginCredentials loginCredentials);