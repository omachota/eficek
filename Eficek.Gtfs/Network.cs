using System.Collections.Frozen;
using System.Collections.ObjectModel;

namespace Eficek.Gtfs;

public class Network
{
	// Using FrozenDictionary since it is faster on benchmarks 
	public required FrozenDictionary<string, Stop> Stops { get; init; }

	public required FrozenDictionary<string, StopGroup> StopGroups { get; init; }

	// List inside needs also to be frozen
	public required FrozenDictionary<(int, int), List<StopGroup>> NearbyStopGroups { get; init; }
	public required ReadOnlyCollection<Node> Nodes { get; init; }
	public required FrozenDictionary<Stop, List<Node>> StopNodes { get; init; }
	public required FrozenDictionary<string, Trip> Trips { get; init; }
}