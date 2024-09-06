namespace Eficek.Models;

public struct CoordinateInfo(double latitude, double longitude)
{
	public double Latitude { get; set; } = latitude;
	public double Longitude { get; set; } = longitude;
}