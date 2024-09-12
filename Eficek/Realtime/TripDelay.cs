using System.Text.Json.Serialization;

namespace Eficek.Realtime;

public class TripDelay
{
	[JsonPropertyName("gtfs_trip_id")]
	public string TripId { get; set; }
	[JsonPropertyName("route_type")]
	public string RouteType { get; set; }
	[JsonPropertyName("gtfs_route_short_name")]
	public string RouteName { get; set; }
	[JsonPropertyName("bearing")]
	public int? Bearing { get; set; }
	[JsonPropertyName("delay")]
	public int? Delay { get; set; }
	[JsonPropertyName("inactive")]
	public bool InActive { get; set; }
	[JsonPropertyName("state_position")]
	public string StatePosition { get; set; }
	[JsonPropertyName("vehicle_id")]
	public string VehicleId { get; set; }
}