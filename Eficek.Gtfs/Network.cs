using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Eficek.Gtfs;

public class Network
{
	// Using FrozenDictionary since it is faster on benchmarks 
	public FrozenDictionary<string, Stop> Stops;
	public FrozenDictionary<string, StopGroup> StopGroups;
	public FrozenDictionary<(int, int), StopGroup[]> NearbyStopGroups;
	public ImmutableArray<Node> Nodes;

	public string FindStops(string stopGroupId)
	{
		throw new NotImplementedException();
	}
}