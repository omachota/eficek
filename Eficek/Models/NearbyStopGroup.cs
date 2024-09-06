using Eficek.Gtfs;

namespace Eficek.Models;

public class NearbyStopGroup(string name, string groupName, double distance) : IFrom<(double, StopGroup), NearbyStopGroup>
{
	public string Name { get; set; } = name;
	public string GroupName { get; set; } = groupName;
	public double Distance { get; set; } = distance;
	public static NearbyStopGroup From((double, StopGroup) from)
	{
		var distance = from.Item1;
		var stopGroup = from.Item2;
		return new NearbyStopGroup(stopGroup.Name, stopGroup.GroupName, distance);
	}
}