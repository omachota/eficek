namespace Eficek.Gtfs;

public static class Constants
{
	/// <summary>
	/// Describes maximal walking distance between stops in metres
	/// </summary>
	public const double MaxStopWalkDistance = 1000;

	/// <summary>
	/// Normal walk speed in km/h
	/// </summary>
	public const double WalkingSpeed = 4;

	/// <summary>
	/// Time it takes to leave a bus (in seconds)
	/// </summary>
	public const int MinTransferTime = 60;
}