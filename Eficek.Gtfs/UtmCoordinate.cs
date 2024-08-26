using System.Runtime.InteropServices;

namespace Eficek.Gtfs;

[StructLayout(LayoutKind.Sequential)]
public struct UtmCoordinate
{
	public double Northing;
	public double Easting;
	public int ZoneNumber; // Max 60

	public (int, int) GetUtmBox()
	{
		var eSq = (int)(Easting / Constants.MaxStopWalkDistance);
		var nSq = (int)(Northing / Constants.MaxStopWalkDistance);

		return (eSq, nSq);
	}
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

	/// <summary>
	/// Determines utm zone
	/// </summary>
	/// <param name="coordinate">WGS84 coordinate with longitude between -180 and 180</param>
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
	/// Assign UtmCoordinate to each stop in the collection. It is way faster than converting one by one
	/// </summary>
	/// <param name="stops"></param>
	public static void AssignUtmCoordinate(IReadOnlyList<Stop> stops)
	{
		var lookup = new Dictionary<int, List<Stop>>();

		for (var i = 0; i < stops.Count; i++)
		{
			// TODO : divide north and south
			var zone = DetermineZone(stops[i].Coordinate);
			if (lookup.TryGetValue(zone, out var list))
			{
				list.Add(stops[i]);
			}
			else
			{
				lookup[zone] = [stops[i]];
			}
		}

		foreach (var zone in lookup.Keys) // Max 60, * 2 with north and south
		{
			var zoneStops = lookup[zone];
			// One allocation might be removed by reusing Coordinate as an input and output to unsafe Convert
			var coordinates = zoneStops.Select(stop => stop.Coordinate).ToArray();
			var utm = new UtmCoordinate[coordinates.Length];
			Convert(coordinates, utm, zone);
			for (var i = 0; i < zoneStops.Count; i++)
			{
				zoneStops[i].UtmCoordinate = utm[i];
			}
		}
	}

	/// <summary>
	/// Convert a single coordinate to UtmCoordinate
	/// </summary>
	/// <param name="coordinate"></param>
	/// <returns></returns>
	public static UtmCoordinate Convert(Coordinate coordinate)
	{
		return ConvertUnsafe(coordinate, (UIntPtr)DetermineZone(coordinate));
	}
}