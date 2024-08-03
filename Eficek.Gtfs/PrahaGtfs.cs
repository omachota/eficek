using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Eficek.Gtfs;

public class PrahaGtfs
{
	private readonly ILogger _logger;

	private static readonly GtfsConfiguration _gtfsConfiguration = new()
	{
		Url = "https://data.pid.cz/PID_GTFS.zip",
		Directory = "Praha",
	};


	public PrahaGtfs(ILogger logger, IConfiguration configuration)
	{
		_logger = logger;
		var value = configuration["PrahaGtfsUrl"];
		if (value != null)
		{
			_logger.Log(LogLevel.Warning, "Overriding default PrahaGtfsUrl");
			_gtfsConfiguration.Url = value;
		}
	}

	public async Task Download()
	{
		using var client = new HttpClient();
		var stream = await client.GetStreamAsync(_gtfsConfiguration.Url);

		// TODO : remove any existing files

		DecompressGtfs(stream, "");
	}
	
	private void DecompressGtfs(Stream stream, string outputDirectory)
	{
		try
		{
			ZipFile.ExtractToDirectory(stream, outputDirectory);
		}
		catch (Exception e)
		{
			_logger.Log(LogLevel.Error, e, "Cannot decompress `todo`");
		}
	}
}
