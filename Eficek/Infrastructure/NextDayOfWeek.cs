namespace Eficek.Infrastructure;

public static class NextDayOfWeekExtension
{
	public static DayOfWeek NextDay(this DayOfWeek dayOfWeek)
	{
		return dayOfWeek.AddDays(1);
	}

	public static DayOfWeek AddDays(this DayOfWeek dayOfWeek, int days)
	{
		return (DayOfWeek)(((int)dayOfWeek + days) % 7);
	}
}