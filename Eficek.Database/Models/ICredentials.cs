namespace Eficek.Database.Models;

public interface ICredentials<T>
{
	public LoginCredentials Credentials { get; set; }
	public T Value { get; set; }
}