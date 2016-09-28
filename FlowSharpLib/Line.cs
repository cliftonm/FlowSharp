using System.Drawing;

namespace FlowSharpLib
{
	public abstract class Line : GraphicElement
	{
		public AvailableLineCap StartCap { get; set; }
		public AvailableLineCap EndCap { get; set; }

		public abstract int X1 { get; }
		public abstract int Y1 { get; }
		public abstract int X2 { get; }
		public abstract int Y2 { get; }

		public Line(Canvas canvas) : base(canvas)
		{
		}

		public override void SnapCheck(ShapeAnchor anchor, Point delta)
		{
			if (canvas.Controller.Snap(anchor.Type, ref delta))
			{
				Move(delta);
			}
			else
			{
				base.SnapCheck(anchor, delta);
			}
		}
	}
}
