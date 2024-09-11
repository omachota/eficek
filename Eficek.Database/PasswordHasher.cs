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
			// There is a problem with call without ms.GetBuffer()
			return SHA256.HashData(ms.GetBuffer());
		}
	}

	public static byte[] Hash(string password, byte[] salt)
	{
		return Hash(Encoding.UTF8.GetBytes(password), salt);
	}
	
	public static bool ValidateHash(byte[] l, byte[] r)
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