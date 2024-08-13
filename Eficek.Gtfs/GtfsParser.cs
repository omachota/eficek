using Microsoft.Extensions.Logging;
using nietras.SeparatedValues;

namespace Eficek.Gtfs;

/// <summary>
/// Gtfs parse should unzip *.gtfs file. The idea for parsing is that names of inner files will be specified, *.gtfs file unzipped, file read and
/// parsed and loaded to memory.
///
/// Every gtfs location might have its own parser (done be inheriting from this call, so it should be abstract...). In location classes,
/// we should override file names.
/// </summary>
public class GtfsParser(GtfsDescription description, ILogger logger)
{
	protected readonly ILogger _logger = logger;

	public List<T> Parse<T>(string filePath) where T : IFromRow<T>
	{
		var path = Path.Combine(description.FullGtfsDirectory, filePath);
		using var reader = Sep.New(',').Reader().FromFile(path);
		var data = new List<T>();
		var x = Sep.New(',').Reader().From(new MemoryStream(new byte[32]));
		x.
		
		foreach (var row in reader)
		{
			data.Add(T.FromRow(row));
		}

		return data;
	}

	protected List<Agency> ParseAgencies()
	{
		return Parse<Agency>(description.Agency);
	}

	protected List<Stop> ParseStops()
	{
		return Parse<Stop>(description.Stops);
	}

	public FeedInfo ParseFeedInfo()
	{
		return Parse<FeedInfo>(description.FeedInfo)[0];
	}
}
