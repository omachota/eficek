using Eficek.Gtfs;

namespace Eficek.Services;

public class StopsService(NetworkService networkService)
{
	// helps to remove code depth
	private readonly (int, int)[] _neighbours = [(-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 0), (0, 1), (1, -1), (1, 0), (1, 1)];

	public IReadOnlyList<StopGroup> GetNearby(Coordinate coordinate)
	{
		var utm = UtmCoordinateBuilder.Convert(coordinate);
		var (eBox, nBox) = utm.GetUtmBox();

		var nearby = new List<StopGroup>();

		for (var i = 0; i < _neighbours.Length; i++)
		{
			// check neighbour box
			if (!networkService.Network.NearbyStopGroups.TryGetValue(
				    (eBox + _neighbours[i].Item1, nBox + _neighbours[i].Item2),
				    out var stopGroups))
			{
				continue;
			}

			// iterate over stopGroups in neighbour box
			for (var j = 0; j < stopGroups.Count; j++)
			{
				if (stopGroups[j].UtmCoordinate.Manhattan(utm) <= Constants.MaxStopWalkDistance)
				{
					nearby.Add(stopGroups[j]);
				}
			}
		}

		return nearby;
	}
}