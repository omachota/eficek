namespace Eficek.Gtfs;

public class GtfsConfiguration
{
	private static readonly string _gtfsCoreDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
	public string Url { get; set; }
	public string Directory { get; init; }
	public string FeedInfo { get; init; }
}
