namespace Eficek.Gtfs;

// LocationType according to specification here: https://gtfs.org/schedule/reference/#stopstxt
public enum LocationType
{
	Stop,
	Station,
	EntranceExit,
	GenericNode,
	BoardingArea
}