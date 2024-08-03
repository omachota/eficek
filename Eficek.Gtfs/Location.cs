namespace Eficek.Gtfs;

public struct Coordinate
{
	public double Latitude;
	public double Longtitude;

	public Coordinate(double latitude, double longtitude)
	{
		Latitude = latitude;
		Longtitude = longtitude;
	}

	public double ManhattanDistanceFrom(Coordinate other)
	{
		throw new NotImplementedException();
	} 
}
