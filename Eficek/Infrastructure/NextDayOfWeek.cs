namespace Eficek.Infrastructure;

public static class NextDayOfWeekExtension
{
	public static DayOfWeek NextDay(this DayOfWeek dayOfWeek)
	{
		return (DayOfWeek)(((int)dayOfWeek + 1) % 7);
	}
}