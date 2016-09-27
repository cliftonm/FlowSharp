using System.Drawing;

namespace FlowSharpLib
{
	public interface ILine
	{
		AvailableLineCap StartCap { get; set; }
		AvailableLineCap EndCap { get; set; }
		Rectangle DisplayRectangle { get; set; }

		int X1 { get; }
		int Y1 { get; }
		int X2 { get; }
		int Y2 { get; }
	}
}
