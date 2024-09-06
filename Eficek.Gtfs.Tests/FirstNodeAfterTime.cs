namespace Eficek.Gtfs.Tests;

public class FirstNodeAfterTime
{
	private static readonly List<Node> _nodes =
	[
		new Node(0, StopHelper.StopWithId("test1"), 10, Node.State.DepartingFromStop),
		new Node(1, StopHelper.StopWithId("test2"), 20, Node.State.DepartingFromStop),
		new Node(2, StopHelper.StopWithId("test3"), 40, Node.State.DepartingFromStop),
		new Node(3, StopHelper.StopWithId("test4"), 150, Node.State.DepartingFromStop),
		new Node(4, StopHelper.StopWithId("test5"), 200, Node.State.DepartingFromStop),
		new Node(5, StopHelper.StopWithId("test6"), 410, Node.State.DepartingFromStop),
		new Node(6, StopHelper.StopWithId("test7"), 550, Node.State.DepartingFromStop),
		new Node(7, StopHelper.StopWithId("test8"), 860, Node.State.DepartingFromStop),
		new Node(8, StopHelper.StopWithId("test9"), 862, Node.State.DepartingFromStop),
	];

	[Theory]
	[InlineData(0, 10)]
	[InlineData(0, -7)]
	[InlineData(3, 150)]
	[InlineData(5, 210)]
	[InlineData(8, 862)]
	[InlineData(0, 900)]
	public void Test(int correctIndex, int searchValue)
	{
		var idx = NodeSearch.IndexOfFirstAfter(_nodes, searchValue);
		Assert.Equal(correctIndex, idx);
	}
}