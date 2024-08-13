using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class Agency(
	string agencyId,
	string agencyName,
	string agencyUrl,
	string agencyTimezone,
	string agencyLang,
	string agencyPhone)
	: IFromRow<Agency>
{
	public string AgencyId = agencyId;
	public string AgencyName = agencyName;
	public string AgencyUrl = agencyUrl;
	public string AgencyTimezone = agencyTimezone;
	public string AgencyLang = agencyLang;
	public string AgencyPhone = agencyPhone;

	public static Agency FromRow(SepReader.Row row)
	{
		return new Agency
		(
			row["agency_id"].ToString(),
			row["agency_name"].ToString(),
			row["agency_url"].ToString(),
			row["agency_timezone"].ToString(),
			row["agency_lang"].ToString(),
			row["agency_phone"].ToString()
		);
	}
}