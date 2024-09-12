using Eficek.Gtfs;

namespace Eficek.Services;

public class NetworkService(NetworkSingletonService networkSingletonService)
{
	public Network Network { get; } = networkSingletonService.Get()!;
}