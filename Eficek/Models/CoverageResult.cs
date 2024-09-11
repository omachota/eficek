using Eficek.Gtfs;

namespace Eficek.Models;

public class CoverageResult(int duration, string stopName) : IFrom<(Node, int), CoverageResult>
{
	public int Duration { get; init; } = duration;
	public string StopName { get; init; } = stopName;

	public static CoverageResult From((Node, int) from)
	{
		return new CoverageResult(from.Item2, from.Item1.Stop.StopName);
	}
}