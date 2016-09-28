using System.Drawing;

namespace FlowSharpLib
{
	public abstract class Line : Connector
	{
		public abstract int X1 { get; }
		public abstract int Y1 { get; }
		public abstract int X2 { get; }
		public abstract int Y2 { get; }

		public Line(Canvas canvas) : base(canvas)
		{
		}

		public override ElementProperties CreateProperties()
		{
			return new LineProperties(this);
		}

		public override bool SnapCheck(ShapeAnchor anchor, Point delta)
		{
			bool ret = canvas.Controller.Snap(anchor.Type, ref delta);

			if (ret)
			{
				Move(delta);
			}
			else
			{
				ret = base.SnapCheck(anchor, delta);
			}

			return ret;
		}

		public override bool SnapCheck(GripType gt, ref Point delta)
		{
			return canvas.Controller.Snap(GripType.None, ref delta);
		}

		public override void MoveElementOrAnchor(GripType gt, Point delta)
		{
			canvas.Controller.MoveElement(this, delta);
		}
	}
}
