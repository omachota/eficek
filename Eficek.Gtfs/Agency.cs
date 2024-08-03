using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class Agency : IFromRow<Agency>
{
	public Agency(string agencyId, string agencyName, string agencyUrl, string agencyTimezone, string agencyLang,
	              string agencyPhone)
	{
		AgencyId = agencyId;
		AgencyName = agencyName;
		AgencyUrl = agencyUrl;
		AgencyTimezone = agencyTimezone;
		AgencyLang = agencyLang;
		AgencyPhone = agencyPhone;
	}

	public string AgencyId;
	public string AgencyName;
	public string AgencyUrl;
	public string AgencyTimezone;
	public string AgencyLang;
	public string AgencyPhone;

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