namespace Eficek.Gtfs;

public struct Coordinate(double latitude, double longitude)
{
	/// <summary>
	/// Longitude in degrees
	/// </summary>
	public readonly double Longitude = longitude;
	
	/// <summary>
	/// Latitude in degrees
	/// </summary>
	public readonly double Latitude = latitude;
	
	public static Coordinate operator +(Coordinate l, Coordinate r)
	{
		return new Coordinate(l.Longitude + r.Longitude, l.Latitude + r.Latitude);
	}

	public static Coordinate operator -(Coordinate l, Coordinate r)
	{
		return new Coordinate(l.Longitude - r.Longitude, l.Latitude - r.Latitude);
	}
}