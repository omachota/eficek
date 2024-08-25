using System.Runtime.InteropServices;

namespace Eficek.Gtfs;

[StructLayout(LayoutKind.Sequential)]
public struct UtmCoordinate
{
	public double Northing;
	public double Easting;
	public int ZoneNumber; // Max 60
}

public static partial class UtmCoordinateBuilder
{
	// Rider suggestion, otherwise use [DllImport] with Cdecl CallConvention
	[LibraryImport("libumt_convert.so", SetLastError = false)]
	[UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
	private static unsafe partial void ConvertArrayUnsafe(Coordinate* ptr_coord, UtmCoordinate* ptr_utm, UIntPtr length,
	                                                      UIntPtr zone);

	[LibraryImport("libumt_convert.so", SetLastError = false)]
	[UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
	private static unsafe partial UtmCoordinate ConvertUnsafe(Coordinate coordinate, UIntPtr zone);

	private static int DetermineZone(Coordinate coordinate)
	{
		return (int)((coordinate.Longitude + 180) / 6) + 1;
	}

	/// <summary>
	/// Convert WGS84 coordinates to UTM coordinates
	/// </summary>
	/// <param name="coordinates">WGS84 coordinates</param>
	/// <param name="utmCoordinates">UTM coordinates</param>
	/// <param name="zone">UTM zone</param>
	public static void Convert(Coordinate[] coordinates, UtmCoordinate[] utmCoordinates, int zone)
	{
		if (coordinates.Length != utmCoordinates.Length)
		{
			throw new ArgumentException($"{nameof(coordinates)} and {nameof(utmCoordinates)} have different length");
		}

		unsafe
		{
			fixed (Coordinate* ptrCoord = coordinates)
			{
				fixed (UtmCoordinate* ptrUTM = utmCoordinates)
				{
					ConvertArrayUnsafe(ptrCoord, ptrUTM, (UIntPtr)coordinates.Length, (UIntPtr)zone);
				}
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="coordinate"></param>
	/// <returns></returns>
	public static UtmCoordinate Convert(Coordinate coordinate)
	{
		return ConvertUnsafe(coordinate, (UIntPtr)DetermineZone(coordinate));
	}
}