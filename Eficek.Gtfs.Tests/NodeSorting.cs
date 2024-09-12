namespace Eficek.Gtfs.Tests;

public class StopNodesSorting
{
	private static readonly Stop _stop = StopHelper.StopWithId("0");

	private static readonly List<Node> _stopNodes =
	[
		new Node(0, _stop, 110, Node.State.DepartingFromStop),
		new Node(1, _stop, 20, Node.State.DepartingFromStop),
		new Node(2, _stop, 800, Node.State.DepartingFromStop),
		new Node(3, _stop, 500, Node.State.DepartingFromStop),
		new Node(4, _stop, 1500, Node.State.DepartingFromStop),
		new Node(5, _stop, 10000, Node.State.DepartingFromStop),
		new Node(6, _stop, 1110, Node.State.DepartingFromStop),
		new Node(7, _stop, 60, Node.State.DepartingFromStop),
		new Node(8, _stop, 400, Node.State.DepartingFromStop),
		new Node(9, _stop, 4000, Node.State.DepartingFromStop),
	];

	[Fact]
	public void Test()
	{
		var cmp = new TimeNodeComparer();
		var expected = new[] { 1, 7, 0, 8, 3, 2, 6, 4, 9, 5 };
		Assert.Equal(expected.Length, _stopNodes.Count);
		_stopNodes.Sort(cmp);
		for (var i = 0; i < expected.Length; i++)
		{
			Assert.Equal(expected[i], _stopNodes[i].InternalId);
		}
	}
}