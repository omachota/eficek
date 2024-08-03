using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public interface IFromRow<out T>
{
	public static abstract T FromRow(SepReader.Row row);
}
