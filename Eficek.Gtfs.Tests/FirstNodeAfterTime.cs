namespace Eficek.Gtfs.Tests;

public class FirstNodeAfterTime
{
	private static Node[] _nodes =
	[
		new Node(0, StopHelper.StopWithId(""), 10, Node.State.InStop),
		new Node(1, StopHelper.StopWithId(""), 0, Node.State.InStop),
		new Node(2, StopHelper.StopWithId(""), 0, Node.State.InStop),
		new Node(3, StopHelper.StopWithId(""), 0, Node.State.InStop),
		new Node(4, StopHelper.StopWithId(""), 0, Node.State.InStop),
		new Node(5, StopHelper.StopWithId(""), 0, Node.State.InStop),
		new Node(6, StopHelper.StopWithId(""), 0, Node.State.InStop),
		new Node(7, StopHelper.StopWithId(""), 86000, Node.State.InStop),
		new Node(8, StopHelper.StopWithId(""), 86200, Node.State.InStop),
	];
}