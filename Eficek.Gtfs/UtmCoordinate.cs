using System.Runtime.InteropServices;

namespace Eficek.Gtfs;

[StructLayout(LayoutKind.Sequential)]
public struct UtmCoordinate
{
	public UtmCoordinate()
	{
	}

	private UtmCoordinate(double easting, double northing, int zoneNumber)
	{
		Easting = easting;
		Northing = northing;
		ZoneNumber = zoneNumber;
	}

	public double Northing;
	public double Easting;
	public int ZoneNumber; // Max 60

	public (int, int) GetUtmBox()
	{
		var eSq = (int)(Easting / Constants.MaxStopWalkDistance);
		var nSq = (int)(Northing / Constants.MaxStopWalkDistance);

		return (eSq, nSq);
	}

	public static UtmCoordinate operator +(UtmCoordinate l, UtmCoordinate r)
	{
		if (l.ZoneNumber != r.ZoneNumber)
		{
			throw new ArgumentException(
				$"UtmCoordinates don't have the same zone: left: {l.ZoneNumber}, right {r.ZoneNumber}");
		}

		return new UtmCoordinate(l.Easting + r.Easting, l.Northing + l.Northing, l.ZoneNumber);
	}

	public static UtmCoordinate operator /(UtmCoordinate l, int r)
	{
		return new UtmCoordinate(l.Easting / r, l.Northing / r, l.ZoneNumber);
	}
}