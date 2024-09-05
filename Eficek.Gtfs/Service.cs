using nietras.SeparatedValues;

namespace Eficek.Gtfs;

// https://gtfs.org/schedule/reference/#calendartxt
// True for an arbitrary day (e.g. Monday) means the service operates on all days (e.g. Mondays) in specified date range
// Exceptions are specified in calendar_dates.txt
public class Service(
	string serviceId,
	bool monday,
	bool tuesday,
	bool wednesday,
	bool thursday,
	bool friday,
	bool saturday,
	bool sunday,
	DateOnly startDate,
	DateOnly endDate)
	: IFromRow<Service>
{
	public static Service Walking()
	{
		var today = DateTime.Today;
		var date = new DateOnly(today.Year, today.Month, today.Day);
		date.AddMonths(1);
		return new Service("1111111-walk", true, true, true, true, true, true, true, date, date.AddMonths(1));
	}

	public readonly string ServiceId = serviceId;
	public readonly bool Monday = monday;
	public readonly bool Tuesday = tuesday;
	public readonly bool Wednesday = wednesday;
	public readonly bool Thursday = thursday;
	public readonly bool Friday = friday;
	public readonly bool Saturday = saturday;
	public readonly bool Sunday = sunday;
	public DateOnly StartDate = startDate;
	public DateOnly EndDate = endDate;

	public static Service FromRow(SepReader.Row row)
	{
		return new Service(
			row["service_id"].ToString(),
			row["monday"].ToString()[0] == '1',
			row["tuesday"].ToString()[0] == '1',
			row["wednesday"].ToString()[0] == '1',
			row["thursday"].ToString()[0] == '1',
			row["friday"].ToString()[0] == '1',
			row["saturday"].ToString()[0] == '1',
			row["sunday"].ToString()[0] == '1',
			DateOnly.ParseExact(row["start_date"].ToString(), "yyyyMMdd"),
			DateOnly.ParseExact(row["end_date"].ToString(), "yyyyMMdd")
		);
	}
}