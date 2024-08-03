namespace Eficek.Gtfs;

public struct Coordinate
{
	public double Latitude;
	public double Longitude;

	public Coordinate(double latitude, double longitude)
	{
		Latitude = latitude;
		Longitude = longitude;
	}

	public double ManhattanDistanceFrom(Coordinate other)
	{
		throw new NotImplementedException();
	} 
}
