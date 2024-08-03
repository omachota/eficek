using nietras.SeparatedValues;

namespace Eficek.Gtfs;

public interface IFromRow<T>
{
	public static abstract T FromRow(SepReader.Row row);
}
