namespace Eficek.Gtfs;

public static class StopGroupExtension
{
	/// <summary>
	/// Extracts group name from stopId. Group name consists of U[0-9]+
	/// </summary>
	/// <returns>The group name of a given stop</returns>
	/// <exception cref="ArgumentException">Throws if stopId violates defined format</exception>
	public static string GroupName(this Stop stop)
	{
		var stopId = stop.StopId;
		if (!stopId.StartsWith('U') || stopId.Length <= 1 || stopId[1] < '0' || stopId[1] > '9')
		{
			throw new ArgumentException($"{stopId} is not a valid stopId");
		}

		var lastNodeNumber = 1;
		while (stopId[lastNodeNumber] >= '0' && stopId[lastNodeNumber] <= '9')
		{
			lastNodeNumber++;
			if (lastNodeNumber >= stopId.Length)
				break;
		}

		return stopId[..lastNodeNumber];
	}
}