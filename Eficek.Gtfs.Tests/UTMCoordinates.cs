namespace Eficek.Gtfs.Tests;

public class UTMCoordinates
{
	[Fact]
	public void Test()
	{
		var coord = new Coordinate(50.0875, 14.4213889);
		var utm = new UtmCoordinate[3];
		var res = UtmCoordinateBuilder.Convert(coord);
		
	}
}