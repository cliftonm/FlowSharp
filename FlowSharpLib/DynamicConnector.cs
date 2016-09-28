using System.Drawing;

namespace FlowSharpLib
{
	public abstract class DynamicConnector : GraphicElement
	{
		public AvailableLineCap StartCap { get; set; }
		public AvailableLineCap EndCap { get; set; }

		public DynamicConnector(Canvas canvas) : base(canvas)
		{
		}

		public override void SnapCheck(ShapeAnchor anchor, Point delta)
		{
			if (canvas.Controller.Snap(anchor.Type, ref delta))
			{
				MoveAnchor(anchor.Type, delta);
			}
			else
			{
				base.SnapCheck(anchor, delta);
			}
		}
	}
}
