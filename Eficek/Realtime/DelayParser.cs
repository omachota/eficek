using System.IO.Compression;
using System.Text.Json;
using Eficek.Services;

namespace Eficek.Realtime;

public class DelayService(NetworkSingletonService ns, ILogger<DelayService> logger) : BackgroundService
{
	// Here are the most compact data : https://mapa.pid.cz/getData.php
	private const string _url =
		"https://api.golemio.cz/v2/public/vehiclepositions?boundingBox=51%2C17%2C48%2C12&routeType=tram&routeType=metro&routeType=train&routeType=bus&routeType=ferry&routeType=funicular&routeType=trolleybus";

	private static readonly HttpRequestMessage _request = new(HttpMethod.Get, _url)
	{
		Headers = { { "accept", "application/json; charset=utf-8'" }, { "Accept-Encoding", "gzip" } }
	};

	public async Task<DelayJsonResponse?> GetDelays()
	{
		using (var client = new HttpClient())
		{
			var response = await client.SendAsync(_request);

			await using var stream = await response.Content.ReadAsStreamAsync();
			await using var decompressed = new GZipStream(stream, CompressionMode.Decompress);
			await using var ms = new MemoryStream();
			await decompressed.CopyToAsync(ms);

			ms.Seek(0, SeekOrigin.Begin);
			return await JsonSerializer.DeserializeAsync<DelayJsonResponse>(ms);
		}
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var pc = new PeriodicTimer(TimeSpan.FromSeconds(20));
		while (!stoppingToken.IsCancellationRequested && !ns.IsBeingUpdated && await pc.WaitForNextTickAsync(stoppingToken))
		{
			var network = ns.Get();
			if (network == null)
			{
				continue;
			}
			
			var delays = await GetDelays();
			if (delays == null)
			{
				logger.LogError("Failed to get delays");
				continue;
			}

			for (var i = 0; i < delays.Features.Length; i++)
			{
				var tripDelay = delays.Features[i].TripDelay;
				network.Trips[tripDelay.TripId].Delay = tripDelay.Delay;
			}
			
			logger.LogInformation("Delays updated");
		}
	}
}