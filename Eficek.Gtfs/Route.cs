using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class Route(
	string routeId,
	string agencyId,
	string routeShortName,
	int routeType,
	string routeColor,
	string routeTextColor)
	: IFromRow<Route>
{
	public readonly string RouteId = routeId;
	public readonly string AgencyId = agencyId;
	public readonly string RouteShortName = routeShortName;
	public readonly int RouteType = routeType;
	public readonly string RouteColor = routeColor;
	public readonly string RouteTextColor = routeTextColor;


	public static Route FromRow(SepReader.Row row)
	{
		return new Route(
			row["route_id"].ToString(),
			row["agency_id"].ToString(),
			row["route_short_name"].ToString(),
			row["route_type"].Parse<int>(),
			row["route_color"].ToString(),
			row["route_text_color"].ToString()
		);
	}
}