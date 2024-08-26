namespace Eficek.Gtfs;

public static class CoordinateExtension
{
	public const double LongitudeAxis = 6378137;
	public const double LatitudeAxis = 6356752.314245;

	/// <summary>
	/// Calculate Manhattan distance between given UtmCoordinates
	/// </summary>
	/// <param name="coordinate"></param>
	/// <param name="other"></param>
	/// <returns>Manhattan distance in metres</returns>
	
	public static double Manhattan(this UtmCoordinate coordinate, UtmCoordinate other)
	{
		if (coordinate.ZoneNumber != other.ZoneNumber)
		{
			return int.MaxValue;
		}
		var dn = Math.Abs(coordinate.Northing - other.Northing);
		var de = Math.Abs(coordinate.Easting - other.Easting);

		return dn + de;
	}
}