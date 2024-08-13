using Eficek.Gtfs;

namespace Eficek.Services;

public class NetworkService
{
	// Can't be implemented as a property
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