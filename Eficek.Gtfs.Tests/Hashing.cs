using Eficek.Database;

namespace Eficek.Gtfs.Tests;

public class Hashing
{
	[Theory]
	[InlineData(true, new byte[] { }, new byte[] { })]
	[InlineData(false, new byte[] { 120 }, new byte[] { })]
	[InlineData(false, new byte[] { }, new byte[] { 54 })]
	[InlineData(true, new byte[] { 56, 91, 23, 0, 87, 39, 0, 129 }, new byte[] { 56, 91, 23, 0, 87, 39, 0, 129 })]
	public void TestHashEquality(bool expectedResult, byte[] l, byte[] r)
	{
		Assert.Equal(expectedResult, PasswordHasher.ValidateHash(l, r));
	}
}