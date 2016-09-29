using System.Collections.Generic;
using System.Drawing;

namespace FlowSharpLib
{
	public abstract class DynamicConnector : Connector
	{
		protected List<Line> lines = new List<Line>();
		protected Point startPoint;
		protected Point endPoint;

		public override bool Selected
		{
			get { return base.Selected; }
			set
			{
				base.Selected = value;
				lines.ForEach(l => l.ShowLineAsSelected = value);
			}
		}

		public DynamicConnector(Canvas canvas) : base(canvas)
		{
			HasCornerAnchors = false;
			HasCenterAnchors = false;
			HasTopBottomAnchors = false;
			HasLeftRightAnchors = false;
		}

		public override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					lines.ForEach(l => l.Dispose());
				}
			}

			base.Dispose(disposing);
		}

		public override void Serialize(ElementPropertyBag epb)
		{
			base.Serialize(epb);
			epb.StartPoint = startPoint;
			epb.EndPoint = endPoint;
		}

		public override void Deserialize(ElementPropertyBag epb)
		{
			base.Deserialize(epb);
			startPoint = epb.StartPoint;
			endPoint = epb.EndPoint;
		}

		public override ElementProperties CreateProperties()
		{
			return new DynamicConnectorProperties(this);
		}

		public override void UpdateProperties()
		{
			lines.ForEach(l =>
			{
				l.BorderPen.Dispose();
				l.BorderPen = new Pen(BorderPen.Color, BorderPen.Width);
			});
		}

		public override bool SnapCheck(ShapeAnchor anchor, Point delta)
		{
			bool ret = canvas.Controller.Snap(anchor.Type, ref delta);

			if (ret)
			{
				MoveAnchor(anchor.Type, delta);
			}
			else
			{
				ret = base.SnapCheck(anchor, delta);
			}

			return ret;
		}


		public override void MoveElementOrAnchor(GripType gt, Point delta)
		{
			MoveAnchor(gt, delta);
		}
	}
}
