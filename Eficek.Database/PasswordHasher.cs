using System.Security.Cryptography;
using System.Text;

namespace Eficek.Database;

public static class PasswordHasher
{
	public const int SaltByteLength = 32;

	public static byte[] Salt()
	{
		return RandomNumberGenerator.GetBytes(SaltByteLength);
	}

	public static byte[] Hash(byte[] password, byte[] salt)
	{
		using (var ms = new MemoryStream(password.Length + salt.Length))
		{
			ms.Write(password, 0, password.Length);
			ms.Write(salt, 0, salt.Length);
			return SHA256.HashData(ms);
		}
	}

	public static byte[] Hash(string password, byte[] salt)
	{
		return Hash(Encoding.UTF8.GetBytes(password), salt);
	}
}