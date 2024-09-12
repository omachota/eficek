namespace Eficek.Gtfs;


// Should be used when different utm zones are present
public struct NearbySquare(int eSquare, int nSquare)
{
	// public int LonZone;
	// public int LatZoneY;
	public int ESquare = eSquare;
	public int NSquare = nSquare;
}