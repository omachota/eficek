namespace Eficek.Gtfs.Tests;

public class UTMCoordinates
{
	// Values taken from:
	// - https://www.dcode.fr/utm-coordinates
	// - https://coordinates-converter.com/en/decimal/50.087465,14.421254?karte=OpenStreetMap&zoom=8

	[Fact]
	public void TestArray()
	{
		var coordinates = new Coordinate[] { new(50.199955, 14.678641), new(50.087465, 14.421254) };
		var utm = new UtmCoordinate[coordinates.Length];
		var utmCorrect = new UtmCoordinate[]
		{
			new() { Easting = 477064, Northing = 5560912, ZoneNumber = 33 },
			new() { Easting = 458598, Northing = 5548515, ZoneNumber = 33 }
		};
		UtmCoordinateBuilder.Convert(coordinates, utm, 33);

		TestUtmCoordinates(utmCorrect, utm);
	}
	
	[Theory]
	[InlineData(33, 477064, 5560912, 50.199955, 14.678641)]
	[InlineData(33, 458598, 5548515, 50.087465, 14.421254)]
	public void TestSingle(int zone, int easting, int northing, double lat, double lon)
	{
		var coord = new Coordinate(lat, lon);
		var utm = coord.ToUtm();
		TestUtmCoordinate(new UtmCoordinate { Easting = easting, Northing = northing, ZoneNumber = zone }, utm);
	}

	private static void TestUtmCoordinates(IList<UtmCoordinate> expected, IList<UtmCoordinate> actual) 
	{
		Assert.Equal(expected.Count, actual.Count);
		
		for (var i = 0; i < expected.Count; i++)
		{
			TestUtmCoordinate(expected[i], actual[i]);
		}
	}

	private static void TestUtmCoordinate(UtmCoordinate expected, UtmCoordinate actual)
	{
		Assert.Equal(expected.ZoneNumber, actual.ZoneNumber);
		Assert.Equal(expected.Easting, (int)actual.Easting);
		Assert.Equal(expected.Northing, (int)actual.Northing);
	}
}