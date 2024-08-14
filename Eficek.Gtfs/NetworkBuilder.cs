using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class NetworkBuilder
{
	public const int SquareSize = 500;
	
	private List<T> Parse<T>(string filePath) where T : IFromRow<T>
	{
		using var reader = Sep.New(',').Reader().FromFile(filePath);
		var data = new List<T>();
		
		foreach (var row in reader)
		{
			data.Add(T.FromRow(row));
		}

		return data;
	}
	
	private T ParseSingle<T>(string filePath) where T : IFromRow<T>
	{
		using var reader = Sep.New(',').Reader().FromFile(filePath);

		if (reader.MoveNext())
		{
			return T.FromRow(reader.Current);
		}
		
		// TODO : better exception
		throw new Exception("File does not contain data");
	}
	
	private async Task<Network> Parse<T>(GtfsParser parser) where T : IFromRow<T>
	{
		var x = new Task<List<T>>(() => parser.Parse<T>(""));
		var y = new Task<T>(() => parser.Parse<T>("")[0]);
		var z = Task.Run(() => "st");
		await Task.WhenAll(x, y, z);
		var r = z.Result;
		
		
		return new Network();
	}

	public async Task<Network> BuildAsync()
	{
		var feedTask = Task.Run(() => ParseSingle<FeedInfo>(""));
		var stopsTask = Task.Run(() => Parse<Stop>(""));
		var routesTask = Task.Run(() => Parse<Route>(""));
		var tripsTask = Task.Run(() => Parse<Trip>(""));
		var stopTimesTask = Task.Run(() => Parse<StopTime>(""));
		
		await Task.WhenAll(feedTask, stopsTask, routesTask, tripsTask, stopTimesTask);
		
		
		var network = new Network();
		network.Nodes = [];
		return new Network();
	}
	
	private Dictionary<(int, int), List<StopGroup>> AssignStopGroupsToSquares(IList<StopGroup> stopGroups)
	{
		var dict = new Dictionary<(int, int), List<StopGroup>>();
		for (var i = 0; i < stopGroups.Count; i++)
		{
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