namespace Eficek.Models;

public class NearbyStopGroup(string name, string groupName, double distance)
{
	public string Name { get; set; } = name;
	public string GroupName { get; set; } = groupName;
	public double Distance { get; set; } = distance;
}