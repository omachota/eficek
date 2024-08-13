using nietras.SeparatedValues;

namespace Eficek.Gtfs;

// https://gtfs.org/schedule/reference/#calendartxt
// True for an arbitrary day (e.g. Monday) means the service operates on all days (e.g. Mondays) in specified date range
// Exceptions are specified in calendar_dates.txt
public class Calendar : IFromRow<Calendar>
{
	public string ServiceId;
	public bool Monday;
	public bool Tuesday;
	public bool Wednesday;
	public bool Thursday;
	public bool Friday;
	public bool Saturday;
	public bool Sunday;
	public DateOnly StartDate;
	public DateOnly EndDate;

	public static Calendar FromRow(SepReader.Row row)
	{
		throw new NotImplementedException();
	}
}