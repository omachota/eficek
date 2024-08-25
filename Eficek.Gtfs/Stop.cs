using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class Stop(
	string stopId,
	string stopName,
	Coordinate coordinate,
	string zoneId,
	string stopUrl,
	LocationType locationType,
	string parentStation,
	int wheelchairBoarding,
	string levelId,
	string platformCode,
	string aswNodeId,
	string aswStopId,
	int? zoneRegionType)
	: IFromRow<Stop>
{
	public string StopId = stopId;
	public string StopName = stopName;

	public Coordinate Coordinate = coordinate;
	public UTMCoordinate UTM;

	// This has to be a string since `P`, `B`, `2,3` and so
	public string ZoneId = zoneId;
	public string StopUrl = stopUrl;
	public LocationType LocationType = locationType;
	public string ParentStation = parentStation;
	public int WheelchairBoarding = wheelchairBoarding;
	public string LevelId = levelId;
	public string PlatformCode = platformCode;
	public string AswNodeId = aswNodeId;
	public string AswStopId = aswStopId;
	public int? ZoneRegionType = zoneRegionType;

	public static Stop FromRow(SepReader.Row row)
	{
		return new Stop(
			row["stop_id"].ToString(),
			row["stop_name"].ToString(),
			new Coordinate(row["stop_lat"].Parse<double>(), row["stop_lon"].Parse<double>()),
			row["zone_id"].ToString(),
			row["stop_url"].ToString(),
			(LocationType)row["location_type"].Parse<int>(),
			row["zone_id"].ToString(), // Todo : fix
			row["wheelchair_boarding"].Parse<int>(),
			row["level_id"].ToString(),
			row["platform_code"].ToString(),
			row["asw_node_id"].ToString(),
			row["asw_stop_id"].ToString(),
			// TryParse needed, documentation allows this to be a number or null :(
			row["zone_region_type"].TryParse<int>()
		);
	}
}