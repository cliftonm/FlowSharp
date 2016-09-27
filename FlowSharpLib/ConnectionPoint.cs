using System.Drawing;

namespace FlowSharpLib
{
	public enum GripType
	{
		None,
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
		public GripType Type { get; protected set; }
		public Point Point { get; protected set; }

		public ConnectionPoint(GripType pos, Point p)
		{
			Type = pos;
			Point = p;
		}
	}
}
