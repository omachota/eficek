namespace Eficek.Gtfs.Tests;

public class UTMCoordinates
{
	[Theory]
	public void Test()
	{
		var coords = new Coordinate[3];
		var res = UTMCoordinateBuilder.Generate(coords);
		
	}
}