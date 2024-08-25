using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Eficek.Gtfs;

public class PragueGtfs
{
	private readonly ILogger _logger;
	private readonly GtfsParser _parser;

	private static readonly GtfsConfiguration _gtfsConfiguration = new()
	{
		Url = "https://data.pid.cz/PID_GTFS.zip",
		Directory = "Prague",
	};

	private readonly GtfsDescription _gtfsDescription;


	public PragueGtfs(ILogger logger, IConfiguration configuration, string globalGtfsDirectory)
	{
		_logger = logger;
		_gtfsDescription = new GtfsDescription(globalGtfsDirectory, "Prague");
		_parser = new GtfsParser(_gtfsDescription, _logger);
		var value = configuration["PragueGtfsUrl"];
		if (value != null)
		{
			_logger.LogWarning("Overriding default PragueGtfsUrl");
			_gtfsConfiguration.Url = value;
		}
	}

	/// <summary>
	/// Downloads Gtfs data if feed_start_date from feed_info.txt is not today or feed_info.txt doesn't exist
	/// </summary>
	/// <param name="force"></param>
	public async ValueTask Download(bool force = false)
	{
		if (!Directory.Exists(_gtfsDescription.FullGtfsDirectory))
		{
			Directory.CreateDirectory(_gtfsDescription.FullGtfsDirectory);
		}
		
		if (File.Exists(Path.Combine(_gtfsDescription.FullGtfsDirectory, _gtfsDescription.FeedInfo)) && !force)
		{
			var feedInfo = _parser.ParseFeedInfo();
			var today = DateOnly.FromDateTime(DateTime.Today);

			if (today == feedInfo.FeedStartDate)
			{
				_logger.LogInformation("Prague gtfs data up to date");
				return;
			}
		}

		// Clear before downloading
		foreach (var file in Directory.EnumerateFiles(_gtfsDescription.FullGtfsDirectory))
		{
			File.Delete(file);
		}

		using var client = new HttpClient();
		var stream = await client.GetStreamAsync(_gtfsConfiguration.Url);

		// TODO : remove any existing files
		DecompressGtfs(stream, _gtfsDescription.FullGtfsDirectory);
		_logger.LogInformation("Prague gtfs data downloaded successfully");
	}

	private void DecompressGtfs(Stream stream, string outputDirectory)
	{
		try
		{
			ZipFile.ExtractToDirectory(stream, outputDirectory);
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Cannot decompress `todo`");
		}
	}
}