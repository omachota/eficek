using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public class Stop : IFromRow<Stop>
{
	public Stop(string stopId, string stopName, Coordinate coordinate, string zoneId, string stopUrl,
	            LocationType locationType, string parentStation, int wheelchairBoarding, string levelId,
	            string platformCode, string aswNodeId, string aswStopId, int zoneRegionType)
	{
		StopId = stopId;
		StopName = stopName;
		Coordinate = coordinate;
		ZoneId = zoneId;
		StopUrl = stopUrl;
		LocationType = locationType;
		ParentStation = parentStation;
		WheelchairBoarding = wheelchairBoarding;
		LevelId = levelId;
		PlatformCode = platformCode;
		AswNodeId = aswNodeId;
		AswStopId = aswStopId;
		ZoneRegionType = zoneRegionType;
	}

	public string StopId;
	public string StopName;

	public Coordinate Coordinate;

	// This has to be a string since `P`, `B`, `2,3` and so
	public string ZoneId;
	public string StopUrl;
	public LocationType LocationType;
	public string ParentStation;
	public int WheelchairBoarding;
	public string LevelId;
	public string PlatformCode;
	public string AswNodeId;

	public string AswStopId;

	// We can ignore for now
	public int ZoneRegionType;

	public static Stop FromRow(SepReader.Row row)
	{
		return new Stop(
			row["stop_id"].ToString(),
			row["stop_name"].ToString(),
			new Coordinate(row["stop_lat"].Parse<double>(), row["stop_lon"].Parse<double>()),
			row["zone_id"].ToString(),
			row["stop_url"].ToString(),
			(LocationType)row["location_type"].Parse<int>(),
			row["zone_id"].ToString(),
			row["wheelchair_boarding"].Parse<int>(),
			row["level_id"].ToString(),
			row["platform_code"].ToString(),
			row["asw_node_id"].ToString(),
			row["asw_stop_id"].ToString(),
			row["zone_region_type"].Parse<int>()
		);
	}
}