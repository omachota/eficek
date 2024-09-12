namespace Eficek.Infrastructure;

public static class DayHelper
{
	public static DayOfWeek NextDay(this DayOfWeek dayOfWeek)
	{
		return dayOfWeek.AddDays(1);
	}

	public static DayOfWeek AddDays(this DayOfWeek dayOfWeek, int days)
	{
		return (DayOfWeek)(((int)dayOfWeek + days) % 7);
	}

	/// <summary>
	/// Returns total seconds from midnight plus DayOfWeek
	/// </summary>
	public static (int, DayOfWeek) SearchTimeInformation(this DateTime dateTime)
	{
		return (dateTime.Hour * 3600 + dateTime.Minute * 60 + dateTime.Second, dateTime.DayOfWeek);
	}
}