using Eficek.Gtfs;

namespace Eficek.Services;

public class NetworkSingletonService(ILogger<NetworkSingletonService> logger)
{
	private readonly SemaphoreSlim _lock = new(1);
	private Network _network = null!;

	public Network Get()
	{
		return _network;
	}

	// Should be locked while updating to prevent simultaneous updates
	public async ValueTask Update(string path)
	{
		if (!await _lock.WaitAsync(0))
		{
			logger.LogWarning("Network update was declined due to other ongoing update");
			return;
		}
		
		var networkBuilder = new NetworkBuilder(path);
		var network = await networkBuilder.BuildAsync(logger);
		
		Interlocked.Exchange(ref _network, network);
		_lock.Release();
	}
}