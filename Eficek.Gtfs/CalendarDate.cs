using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class CalendarDate : IFromRow<CalendarDate>
{
	public string ServiceId;
	public DateOnly Date;
	/// <summary>
	/// Replaces exception_type:
	///		- true (1): service has been added
	///		- false (2): service has been removed
	/// </summary>
	public bool ServiceAvailable;
	
	
	public static CalendarDate FromRow(SepReader.Row row)
	{
		throw new NotImplementedException();
	}
}