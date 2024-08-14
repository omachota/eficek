using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class Route(string routeId, string agencyId, string routeShortName, int routeType)
	: IFromRow<Route>
{
	public string RouteId = routeId;
	public string AgencyId = agencyId;
	public string RouteShortName = routeShortName;
	public int RouteType = routeType;


	public static Route FromRow(SepReader.Row row)
	{
		return new Route(
			row["route_id"].ToString(),
			row["agency_id"].ToString(),
			row["route_short_name"].ToString(),
			row["route_type"].Parse<int>()
		);
	}
}