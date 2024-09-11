using Eficek.Gtfs;

namespace Eficek.Services;

public class NetworkSingletonService(ILogger<NetworkSingletonService> logger, IConfiguration configuration)
{
	private readonly SemaphoreSlim _lock = new(1);
	private Network _network = null!;

	/// <summary>
	/// Returns current search graph
	/// </summary>
	public Network Get()
	{
		return _network;
	}

	/// <summary>
	/// Current API update status
	/// </summary>
	public bool IsBeingUpdated { get; private set; }

	// Locked while updating to prevent simultaneous updates
	/// <summary>
	/// Downloads gtfs data and updates search graph. Only one update is allowed at a time.
	/// </summary>
	/// <param name="path">Global folder for gtfs data</param>
	/// <param name="force">Force true downloads gtfs data from server</param>
	public async ValueTask Update(string path, bool force = true)
	{
		if (!await _lock.WaitAsync(0))
		{
			logger.LogWarning("Network update was declined due to other ongoing update");
			return;
		}

		logger.LogInformation("API started to update");
		IsBeingUpdated = true;
		if (force)
		{
			var pragueGtfs = new PragueGtfs(logger, configuration, path);
			await pragueGtfs.Download();
		}

		var networkBuilder = new NetworkBuilder(path);
		var network = await networkBuilder.BuildAsync(logger);

		Interlocked.Exchange(ref _network, network);
		IsBeingUpdated = false;
		logger.LogInformation("Update finished");
		_lock.Release();
	}
}