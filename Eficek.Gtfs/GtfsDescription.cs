namespace Eficek.Gtfs;

public class GtfsDescription(string globalGtfsDirectory, string directoryName)
{
	public required string Url { get; set; }
	public string FullGtfsDirectory { get; } = Path.Combine(globalGtfsDirectory, directoryName);
	public string FeedInfo { get; set; } = "feed_info.txt";
}
