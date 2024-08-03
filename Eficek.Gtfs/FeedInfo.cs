using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class FeedInfo(
	string feedPublisherName,
	string feedPublisherUrl,
	string feedLang,
	DateOnly feedStartDate,
	DateOnly feedEndDate,
	string feedContactEmail)
	: IFromRow<FeedInfo>
{
	public string FeedPublisherName = feedPublisherName;
	public string FeedPublisherUrl = feedPublisherUrl;
	public string FeedLang = feedLang;
	public DateOnly FeedStartDate = feedStartDate;
	public DateOnly FeedEndDate = feedEndDate;
	public string FeedContactEmail = feedContactEmail;

	public static FeedInfo FromRow(SepReader.Row row)
	{
		return new FeedInfo(
			row["feed_publisher_name"].ToString(),
			row["feed_publisher_url"].ToString(),
			row["feed_lang"].ToString(),
			DateOnly.ParseExact( row["feed_start_date"].Span, "yyyyMMdd"),
			DateOnly.ParseExact( row["feed_end_date"].Span, "yyyyMMdd"),
			row["feed_contact_email"].ToString()
		);
	}
}