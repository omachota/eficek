namespace Eficek.Models;

public class DepartureResult(string tripName, int time)
{
	public string TripName { get; set; } = tripName;
	public int Time { get; set; } = time;

	public string ReadableTime
	{
		get
		{
			var hours = Time / 3600;
			var minutes = (Time - hours * 3600) / 60;
			var seconds = (Time - hours * 3600) / 3600;
			return $"{hours:00}:{minutes:00}:{seconds:00}";
		}
	}
}