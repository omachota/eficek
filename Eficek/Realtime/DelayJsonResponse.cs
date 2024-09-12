using System.Text.Json.Serialization;

namespace Eficek.Realtime;

public class DelayJsonResponse
{
	[JsonPropertyName("features")]
	public Feature[] Features { get; set; }
	[JsonPropertyName("type")]
	public string Type { get; set; }
}