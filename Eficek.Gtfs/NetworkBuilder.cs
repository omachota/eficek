using System.Collections.Immutable;

namespace Eficek.Gtfs;

public class NetworkBuilder
{
	public const int SquareSize = 500;

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
		var parser = new GtfsParser();
		var net = new Network();
		List<FeedInfo> x = null;
		var y = Parse<FeedInfo>(parser);
		net.Nodes = [];
		return new Network();
	}

	private Dictionary<(int, int), List<StopGroup>> Generate(IList<StopGroup> stopGroups)
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
	}
}