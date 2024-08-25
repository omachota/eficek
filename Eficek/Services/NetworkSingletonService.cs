using Eficek.Gtfs;

namespace Eficek.Services;

public class NetworkSingletonService
{
	private Network _network = null!;

	public Network Get()
	{
		return _network;
	}

	// Should be locked while updating
	public async ValueTask Update()
	{
		Interlocked.Exchange(ref _network, new Network());
	}
}