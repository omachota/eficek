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
	public readonly string StopId = stopId;
	public readonly string StopName = stopName;

	public readonly Coordinate Coordinate = coordinate;
	public UtmCoordinate UtmCoordinate;

	// This has to be a string since `P`, `B`, `2,3` and so
	public readonly string ZoneId = zoneId;
	public readonly string StopUrl = stopUrl;
	public readonly LocationType LocationType = locationType;
	public readonly string ParentStation = parentStation;
	public readonly int WheelchairBoarding = wheelchairBoarding;
	public readonly string LevelId = levelId;
	public readonly string PlatformCode = platformCode;
	public readonly string AswNodeId = aswNodeId;
	public readonly string AswStopId = aswStopId;
	public readonly int? ZoneRegionType = zoneRegionType;

	public static Stop FromRow(SepReader.Row row)
	{
		return new Stop(
			row["stop_id"].ToString(),
			row["stop_name"].ToString().Trim('\"'),
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