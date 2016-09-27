using System.Drawing;

namespace FlowSharpLib
{
	public enum ConnectionPosition
	{
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
		LeftMiddle,
		RightMiddle,
		TopMiddle,
		BottomMiddle,

		// for lines:
		Start,
		End,
	};

	public class ConnectionPoint
	{
		public ConnectionPosition Type { get; protected set; }
		public Point Point { get; protected set; }

		public ConnectionPoint(ConnectionPosition pos, Point p)
		{
			Type = pos;
			Point = p;
		}
	}
}
