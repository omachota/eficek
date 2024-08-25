using System.Collections.Frozen;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class NetworkBuilder(string path)
{
	private readonly string _path = path;

	public const int SquareSize = 500;

	private static List<T> Parse<T>(string filePath) where T : IFromRow<T>
	{
		using var reader = Sep.New(',').Reader().FromFile(filePath);
		var data = new List<T>();

		foreach (var row in reader)
		{
			data.Add(T.FromRow(row));
		}

		return data;
	}

	private delegate (string, T) KeySelector<T>(SepReader.Row row);

	private static Dictionary<string, T> ParseDict<T>(string filePath, KeySelector<T> keySelector,
	                                                  Func<T, bool>? predicate = null)
	{
		using var reader = Sep.New(',').Reader().FromFile(filePath);
		var data = new Dictionary<string, T>();

		foreach (var row in reader)
		{
			var (key, value) = keySelector(row);
			if (predicate != null)
			{
				if (!predicate(value))
				{
					continue;
				}
			}

			data.Add(key, value);
		}

		return data;
	}

	private static T ParseSingle<T>(string filePath) where T : IFromRow<T>
	{
		using var reader = Sep.New(',').Reader().FromFile(filePath);

		if (reader.MoveNext())
		{
			return T.FromRow(reader.Current);
		}

		// TODO : better exception
		throw new Exception($"File `{filePath}` does not contain data");
	}

	private readonly KeySelector<Stop> _stopSelector = row =>
	{
		var s = Stop.FromRow(row);
		return (s.StopId, s);
	};

	private readonly Func<Stop, bool> _stopFilter = stop =>
		stop.ZoneRegionType.HasValue == false || stop.ZoneRegionType == 0;

	public async Task<Network> BuildAsync(ILogger logger)
	{
		var stopwatch = Stopwatch.StartNew();
		var feedTask = Task.Run(() => ParseSingle<FeedInfo>(BuildRelativeFilePath("feed_info.txt")));
		var stopsTask = Task.Run(() => ParseDict<Stop>(BuildRelativeFilePath("stops.txt"), _stopSelector, _stopFilter));
		var routesTask = Task.Run(() => Parse<Route>(BuildRelativeFilePath("routes.txt")));
		var tripsTask = Task.Run(() => Parse<Trip>(BuildRelativeFilePath("trips.txt")));
		var stopTimesTask = Task.Run(() => Parse<StopTime>(BuildRelativeFilePath("stop_times.txt")));

		try
		{
			await Task.WhenAll(feedTask, stopsTask, routesTask, tripsTask, stopTimesTask);
		}
		catch (Exception e)
		{
			logger.LogError(e, "Failed to parse gtfs data");
			throw;
		}

		stopwatch.Stop();

		logger.LogInformation(
			"Gtfs data successfully parsed in {} milliseconds\n\tStops: {}\n\tRoutes: {}\n\tTrips: {}\n\tStopTimes: {}",
			stopwatch.ElapsedMilliseconds, stopsTask.Result.Count, routesTask.Result.Count, tripsTask.Result.Count,
			stopTimesTask.Result.Count);

		var network = new Network();
		network.Nodes = [];
		network.Stops = stopsTask.Result.ToFrozenDictionary();
		return network;
	}

	private string BuildRelativeFilePath(string fileName)
	{
		return Path.Combine(_path, "Prague", fileName);
	}

	public Dictionary<string, StopGroup> GenerateStopGroups(IList<Stop> stops)
	{
		var dict = new Dictionary<string, StopGroup>();
		for (var i = 0; i < stops.Count; i++)
		{
			// Todo : GroupName should be precomputed when creating a stop?
			var groupName = stops[i].GroupName();
			if (!dict.TryGetValue(groupName, out var stopGroup))
			{
				stopGroup = new StopGroup { };
			}

			stopGroup.AddStop(stops[i]);
		}

		return dict;
	}

	private Dictionary<(int, int), List<StopGroup>> AssignStopGroupsToSquares(IList<StopGroup> stopGroups)
	{
		var dict = new Dictionary<(int, int), List<StopGroup>>();
		for (var i = 0; i < stopGroups.Count; i++)
		{
			// Since the whole Czech republic is in 33U, we don't have to care about UTM rectangles
			var utm = stopGroups[i].Coordinate.UTM;
			var eSq = (int)(utm.Easting / SquareSize);
			var nSq = (int)(utm.Northing / SquareSize);
			if (dict.TryGetValue((eSq, nSq), out var nearbyStopGroups))
			{
				nearbyStopGroups.Add(stopGroups[i]);
			}
			else
			{
				dict[(eSq, nSq)] = [stopGroups[i]];
			}
		}

		return dict;
	}
}