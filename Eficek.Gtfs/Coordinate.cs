using System.Runtime.InteropServices;

namespace Eficek.Gtfs;

[StructLayout(LayoutKind.Sequential)]
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

	// Operations bellow were meant to help finding the center for a StopGroup
	public static Coordinate operator +(Coordinate l, Coordinate r)
	{
		return new Coordinate(l.Longitude + r.Longitude, l.Latitude + r.Latitude);
	}

	public static Coordinate operator -(Coordinate l, Coordinate r)
	{
		return new Coordinate(l.Longitude - r.Longitude, l.Latitude - r.Latitude);
	}

	public static Coordinate operator /(Coordinate l, int r)
	{
		return new Coordinate(l.Latitude / r, l.Longitude / r);
	}
}