using System.Text.RegularExpressions;
using Eficek.Gtfs;
using Eficek.Models;
using Stop = Eficek.Gtfs.Stop;

namespace Eficek.Services;

public class StopsService(NetworkService networkService)
{
	// helps to remove code depth
	private readonly (int, int)[] _neighbours = [(-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 0), (0, 1), (1, -1), (1, 0), (1, 1)];

	public IReadOnlyList<NearbyStopGroup> GetNearby(Coordinate coordinate)
	{
		var utm = UtmCoordinateBuilder.Convert(coordinate);
		var (eBox, nBox) = utm.GetUtmBox();

		var nearby = new List<NearbyStopGroup>();

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
				var distance = stopGroups[j].UtmCoordinate.Manhattan(utm);
				if (distance <= Constants.MaxStopWalkDistance)
				{
					nearby.Add(new NearbyStopGroup(stopGroups[j].Name, stopGroups[j].GroupName, distance));
				}
			}
		}

		return nearby;
	}

	public IReadOnlyList<StopGroupMatch> Search(string value, int maxResults)
	{
		var r = new Regex($".*{value}.*");

		var matches = new List<StopGroupMatch>();
		foreach (var stopGroup in networkService.Network.StopGroups.Values)
		{
			var match = r.Match(stopGroup.Name);
			if (match.Success && (matches.Count < maxResults || maxResults == -1))
			{
				matches.Add(new StopGroupMatch(stopGroup.Name, stopGroup.GroupName));
			}
		}

		return matches;
	}

	public StopGroup? TryGet(string stopGroupId)
	{
		networkService.Network.StopGroups.TryGetValue(stopGroupId, out var stopGroup);
		return stopGroup;
	}

	public Stop? TryGetStop(string stopId)
	{
		networkService.Network.Stops.TryGetValue(stopId, out var stop);
		return stop;
	}
}