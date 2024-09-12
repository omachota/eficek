using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class CalendarDate(string serviceId, DateOnly date, bool serviceAvailable) : IFromRow<CalendarDate>
{
	public readonly string ServiceId = serviceId;
	public readonly DateOnly Date = date;

	/// <summary>
	/// Replaces exception_type:
	///		- true (1): service has been added
	///		- false (2): service has been removed
	/// </summary>
	public readonly bool ServiceAvailable = serviceAvailable;


	public static CalendarDate FromRow(SepReader.Row row)
	{
		return new CalendarDate(
			row["service_id"].ToString(),
			DateOnly.ParseExact(row["date"].Span, "yyyyMMdd"),
			row["exception_type"].Parse<int>() == 1
		);
	}
}