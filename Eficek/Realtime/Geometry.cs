using System.Text.Json.Serialization;

namespace Eficek.Realtime;

public class Geometry
{
	[JsonPropertyName("coordinates")]
	public double[] Cooridnates { get; set; }

	[JsonPropertyName("type")]
	public string Type { get; set; }
}