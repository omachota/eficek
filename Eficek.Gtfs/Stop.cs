using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class Stop : IFromRow<Stop>
{
	public string StopId;
	public string StopName;
	public Coordinate Coordinate;
	// This has to be string since `P`, `B`, `2,3` and so
	public string ZoneId;
	public static Stop FromRow(SepReader.Row row)
	{
		throw new NotImplementedException();
	}
}
