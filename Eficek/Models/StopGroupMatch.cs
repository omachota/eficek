namespace Eficek.Models;

public class StopGroupMatch(string name, string groupName)
{
	public string Name { get; init; } = name;
	public string GroupName { get; init; } = groupName;
}