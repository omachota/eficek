using System.Collections.Frozen;
using System.Collections.ObjectModel;

namespace Eficek.Gtfs;

public class Network
{
	// Using FrozenDictionary since it is faster on benchmarks 
	public FrozenDictionary<string, Stop> Stops;
	public FrozenDictionary<string, StopGroup> StopGroups;
	// List inside needs also to be frozen
	public FrozenDictionary<(int, int), List<StopGroup>> NearbyStopGroups;
	public ReadOnlyCollection<Node> Nodes;
	public FrozenDictionary<Stop, List<Node>> StopNodes;

	public string FindStops(string stopGroupId)
	{
		throw new NotImplementedException();
	}
}