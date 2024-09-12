using System.Text.Json.Serialization;

namespace Eficek.Realtime;

public class Feature
{
	[JsonPropertyName("geometry")]
	public Geometry Geometry { get; set; }

	[JsonPropertyName("properties")]
	public TripDelay TripDelay { get; set; }
	
	[JsonPropertyName("type")]
	public string Type { get; set; }
}