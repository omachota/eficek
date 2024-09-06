namespace Eficek.Models;

public interface IFrom<in TFrom, out TOut>
{
	public static abstract TOut From(TFrom from);
}