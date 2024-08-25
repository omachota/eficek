using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class StopTime : IFromRow<StopTime>
{
	public string TripId;
	
	
	public static StopTime FromRow(SepReader.Row row)
	{
		return new StopTime();
	}
}