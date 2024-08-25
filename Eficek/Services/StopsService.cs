using Eficek.Gtfs;

namespace Eficek.Services;

public class StopsService(NetworkService networkService)
{
	private const int SquareSize = 500; // In meters

	// helps to remove code depth
	private readonly (int, int)[] _neighbours =
		[(-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 0), (0, 1), (1, -1), (1, 0), (1, 1)];

	public IReadOnlyList<StopGroup> GetNearby(Coordinate coordinate)
	{
		var utm = UtmCoordinateBuilder.Convert(coordinate);
		var eSquare = (int)(utm.Easting / SquareSize);
		var nSquare = (int)(utm.Northing / SquareSize);

		var nearby = new List<StopGroup>();

		for (var i = 0; i < _neighbours.Length; i++)
		{
			if (!networkService.Network.NearbyStopGroups.TryGetValue(
				    (eSquare + _neighbours[i].Item1, nSquare + _neighbours[i].Item2),
				    out var stopGroups))
			{
				continue;
			}

			for (var j = 0; j < stopGroups.Length; j++)
			{
				if (stopGroups[j].Coordinate?.Manhattan(coordinate) <= SquareSize)
				{
					nearby.Add(stopGroups[j]);
				}
			}
		}

		return nearby;
	}
}