namespace Eficek.Gtfs;

public static class Constants
{
	/// <summary>
	/// Describes maximal walking distance between stops in metres
	/// </summary>
	public const double MaxStopWalkDistance = 700;

	/// <summary>
	/// Normal walk speed in m/s
	/// </summary>
	public const double WalkingSpeed = 1.4;

	/// <summary>
	/// Time it takes to leave a bus (in seconds)
	/// </summary>
	public const int MinTransferTime = 60;

	/// <summary>
	/// Helps with search of nearby stations
	/// </summary>
	public static readonly (int, int)[] Neighbours =
		[(-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 0), (0, 1), (1, -1), (1, 0), (1, 1)];
}