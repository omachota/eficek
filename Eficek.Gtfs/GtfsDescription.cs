namespace Eficek.Gtfs;

public class GtfsDescription(string globalGtfsDirectory, string directoryName)
{
	public string FullGtfsDirectory { get; } = Path.Combine(globalGtfsDirectory, directoryName);
	public string Agency { get; set; } = "agency.txt";
	public string Stops { get; set; } = "stops.txt";
	public string FeedInfo { get; set; } = "feed_info.txt";
}
