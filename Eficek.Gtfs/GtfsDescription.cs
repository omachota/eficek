namespace Eficek.Gtfs;

public class GtfsDescription
{
	public GtfsDescription(string globalGtfsDirectory, string directoryName)
	{
		FullGtfsDirectory = Path.Combine(globalGtfsDirectory, directoryName);
	}

	public string FullGtfsDirectory { get; }
	public string Agency { get; set; } = "agency.txt";
	public string Stops { get; set; } = "stops.txt";
	public string FeedInfo { get; set; } = "feed_info.txt";
}
