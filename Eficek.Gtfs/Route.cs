using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class Route : IFromRow<Route>
{
	public string RouteId;
	public string AgencyId;
	public string RouteShortName;
	public int RouteType;


	public Route(string routeId, string agencyId, string routeShortName, int routeType)
	{
		RouteId = routeId;
		AgencyId = agencyId;
		RouteShortName = routeShortName;
		RouteType = routeType;
	}

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