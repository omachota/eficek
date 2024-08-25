namespace Eficek.Gtfs.Tests;

public class StopIdGroupName
{
	[Theory]
	[InlineData(null, "")]
	[InlineData(null, "U")]
	[InlineData(null, "UZ")]
	[InlineData("U125", "U125")]
	[InlineData("U125", "U125Z")]
	[InlineData("U125", "U125ZP12")]
	[InlineData("U1072", "U1072S1")]
	public void Test(string? rootName, string stopId)
	{
		var stop = StopWithId(stopId);
		if (rootName == null)
		{
			Assert.Throws<ArgumentException>(() => stop.GroupName());
		}
		else
		{
			Assert.Equal(rootName, stop.GroupName());
		}
	}

	private static Stop StopWithId(string stopId)
	{
		return new Stop(stopId, "", new Coordinate(0, 0), "", "", LocationType.Stop, "", 0, "", "", "", "", 1);
	}
}