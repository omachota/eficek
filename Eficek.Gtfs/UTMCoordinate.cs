using NumSharp;

namespace Eficek.Gtfs;

public struct UTMCoordinate
{
	public double Northing;
	public double Easting;
	public int ZoneNumber; // Max 60
	public int LatitudeBand; // Max 22?
}

public static class UTMCoordinateBuilder
{
	public static UTMCoordinate[] Generate(IReadOnlyList<Coordinate> coordinates)
	{
		var deconstructedCoords = new double[coordinates.Count][];
		for (var i = 0; i < coordinates.Count; i++)
		{
			deconstructedCoords[i] = [coordinates[i].Latitude, coordinates[i].Longitude]; // TODO : pad 0, 0
		}

		var utm = GenerateInternal(np.array(deconstructedCoords));

		var utmConstructed = new UTMCoordinate[utm.shape[0]];
		for (var i = 0; i < utm.shape[0]; i++)
		{
			utmConstructed[i] = new UTMCoordinate { ZoneNumber = utm[i, 3], LatitudeBand = utm[i, 4] };
		}

		return utmConstructed;
	}

	private static NDArray GenerateInternal(NDArray coordinates)
	{
		np.reshape(coordinates, (coordinates.shape[0], 4));
		var res = np.zeros((coordinates.shape[0], 4));

		#region ZoneNumber from Longitude

		res[":, 3"] = np.floor((coordinates[":, 1"] + 180) / 6) + 1;

		#endregion

		#region LatitudeBand from Latitude

		var latitudes = coordinates[":, 0"];
		var latShift = latitudes + 80;
		// Fixme : conditions are wrong
		// latShift < 0 => 0
		// latShift >= 72+80 => 22
		// latShift >= 84+80 => 23/24
		NDArray ab = latShift < 0;
		NDArray x = latitudes >= 72;
		NDArray yz = latitudes >= 84;
		res[":, 4"] = latShift / 8 + 2; // skip ab
		/*res[ab] = 0;
		res[x] = 22;
		res[yz] = 23; */

		#endregion


		return res;
	}
}