using System.Runtime.InteropServices;

namespace Eficek.Gtfs;

[StructLayout(LayoutKind.Sequential)]
public struct UtmCoordinate
{
	public double Northing;
	public double Easting;
	public int ZoneNumber; // Max 60

	public (int, int) GetUtmBox()
	{
		var eSq = (int)(Easting / Constants.MaxStopWalkDistance);
		var nSq = (int)(Northing / Constants.MaxStopWalkDistance);

		return (eSq, nSq);
	}
}