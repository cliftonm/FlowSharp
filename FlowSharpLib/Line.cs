using System.Drawing;

namespace FlowSharpLib
{
	public abstract class Line : Connector
	{
		public bool ShowLineAsSelected { get; set; }

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
