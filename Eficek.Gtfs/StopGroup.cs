using System.Text;

namespace Eficek.Gtfs;

// TODO : try using something like StopGroupBuilder to get rid of possible runtime exceptions
/// <summary>
/// Contains all stops with the same group name.
/// </summary>
public class StopGroup(string groupId)
{
	private bool _frozen;

	private Coordinate _coordinate;

	// ZoneNumber has to be set not to raise exception
	private UtmCoordinate _utmCoordinate = new() { ZoneNumber = 33 };

	public readonly HashSet<Stop> Stops = [];

	public Coordinate Coordinate
	{
		get
		{
			if (!_frozen)
			{
				throw new UnFrozenException("Access to `Coordinate` is allowed after freezing");
			}

			return _coordinate;
		}
		private set => _coordinate = value;
	}

	public UtmCoordinate UtmCoordinate
	{
		get
		{
			if (!_frozen)
			{
				throw new UnFrozenException("Access to `UtmCoordinate` is allowed after freezing");
			}

			return _utmCoordinate;
		}
		private set => _utmCoordinate = value;
	}

	public string Name { get; private set; }
	public string GroupId { get; } = groupId;

	public bool AddStop(Stop stop)
	{
		if (_frozen)
		{
			return false;
		}

		Stops.Add(stop);
		Name = stop.StopName;
		_coordinate += stop.Coordinate;
		_utmCoordinate += stop.UtmCoordinate;
		return true;
	}

	/// <summary>
	/// After calling this method, stops can't be added anymore
	/// </summary>
	public void Freeze()
	{
		_frozen = true;
		Coordinate /= Stops.Count;
		UtmCoordinate /= Stops.Count;
	}

	public override string ToString()
	{
		var sb = new StringBuilder();
		foreach (var stop in Stops)
		{
			sb.Append(stop.StopName);
			sb.Append(';');
		}

		return sb.ToString();
	}
}

// Or Melted :D
public class UnFrozenException : Exception
{
	public UnFrozenException()
	{
	}

	public UnFrozenException(string message)
		: base(message)
	{
	}

	public UnFrozenException(string message, Exception inner)
		: base(message, inner)
	{
	}
}