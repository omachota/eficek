namespace Eficek.Gtfs.Tests;

public class CoordinatesDistance
{
	[Theory]
	[InlineData(0, 0, 0, 0, 0)]
	[InlineData(CoordinateExtension.LatitudeAxis * Math.PI / 2, 90, 0, 0, 0)]
	[InlineData(CoordinateExtension.LongitudeAxis * Math.PI / 2, 0, 90, 0, 0)]
	[InlineData((CoordinateExtension.LongitudeAxis + CoordinateExtension.LatitudeAxis) * Math.PI / 2, 90, 90, 0, 0)]
	public void Test(double res, double lat1, double lon1, double lat2, double lon2)
	{
		var coord1 = new Coordinate(lat1, lon1);
		var coord2 = new Coordinate(lat2, lon2);
		coord1.Manhattan(coord2);
	}
}