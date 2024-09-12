namespace Eficek.Gtfs.Tests;

public class CoordinatesDistance
{
	[Theory]
	[InlineData(0, 49, 16, 49, 16)]
	[InlineData(1000, 1000, 0, 0, 0)]
	[InlineData(1000, 0, 1000, 0, 0)]
	[InlineData(1000, 0, 0, 1000, 0)]
	[InlineData(1000, 0, 0, 0, 1000)]
	[InlineData(1000, 0, 500, 500, 0)]
	[InlineData(200, 400, 500, 300, 400)]
	public void Test(double res, double e1, double n1, double e2, double n2)
	{
		var utm1 = new UtmCoordinate { Easting = e1, Northing = n1, ZoneNumber = 33 };
		var utm2 = new UtmCoordinate { Easting = e2, Northing = n2, ZoneNumber = 33 };

		Assert.Equal(res, utm1.Manhattan(utm2));
	}
}