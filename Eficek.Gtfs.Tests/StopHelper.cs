namespace Eficek.Gtfs.Tests;

public static class StopHelper
{
	public static Stop StopWithId(string id)
	{
		return new Stop(id, "", new Coordinate(0, 0), "", "", LocationType.Stop, "", 0, "", "", "", "", 1);
	}
}