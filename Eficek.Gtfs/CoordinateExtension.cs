using CoordinateSharp;

namespace Eficek.Gtfs;

public static class CoordinateExtension
{
	public const double LongitudeAxis = 6378137;
	public const double LatitudeAxis = 6356752.314245;
	
	/// <summary>
	/// Calculates Manhattan distance between given coordinates on WGS84
	/// </summary>
	/// <param name="coordinate"></param>
	/// <param name="other"></param>
	/// <returns>Manhattan distance in metres</returns>
	public static double Manhattan(this Coordinate coordinate, Coordinate other)
	{
		var dlat = Math.Abs(coordinate.Latitude.Degrees - other.Latitude.Degrees);
		var dlon = Math.Abs(coordinate.Longitude.Degrees- other.Longitude.Degrees);

		var dlatRad = double.DegreesToRadians(dlat);
		var dlonRad = double.DegreesToRadians(dlon);

		return dlatRad * LatitudeAxis + dlonRad * LongitudeAxis;
	}
}