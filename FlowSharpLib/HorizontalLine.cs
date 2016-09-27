using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FlowSharpLib
{
	public enum AvailableLineCap
	{
		None,
		Arrow,
	};

	public class HorizontalLine : GraphicElement, ILine
	{
		public AvailableLineCap StartCap { get; set; }
		public AvailableLineCap EndCap { get; set; }

		// Fixes background erase issues with dynamic connector.
		public override Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(anchorSize + 1 + BorderPen.Width); } }

		public int X1 { get { return DisplayRectangle.X; } }
		public int Y1 { get { return DisplayRectangle.Y + BaseController.MIN_HEIGHT / 2; } }
		public int X2 { get { return DisplayRectangle.X + DisplayRectangle.Width; } }
		public int Y2 { get { return DisplayRectangle.Y + BaseController.MIN_HEIGHT / 2; } }

		public HorizontalLine(Canvas canvas) : base(canvas)
		{
			HasCornerAnchors = false;
			HasCenterAnchors = false;
			HasLeftRightAnchors = true;
		}

		public override ElementProperties CreateProperties()
		{
			return new LineProperties(this);
		}

		public override GraphicElement Clone(Canvas canvas)
		{
			HorizontalLine line = (HorizontalLine)base.Clone(canvas);
			line.StartCap = StartCap;
			line.EndCap = EndCap;

			return line;
		}

		public override Rectangle DefaultRectangle()
		{
			return new Rectangle(20, 20, 40, 20);
		}

		protected override void Draw(Graphics gr)
		{
			// See CustomLineCap for creating other possible endcaps besides arrows.
			// Note that AdjustableArrowCap derives from CustomLineCap!
			// https://msdn.microsoft.com/en-us/library/system.drawing.drawing2d.customlinecap(v=vs.110).aspx

			AdjustableArrowCap adjCap = new AdjustableArrowCap(5, 5, true);
			Pen pen = (Pen)BorderPen.Clone();

			if (StartCap == AvailableLineCap.Arrow)
			{
				pen.CustomStartCap = adjCap;
			}

			if (EndCap == AvailableLineCap.Arrow)
			{
				pen.CustomEndCap = adjCap;
			}

			gr.DrawLine(pen, DisplayRectangle.LeftMiddle(), DisplayRectangle.RightMiddle());
			pen.Dispose();
			adjCap.Dispose();

			base.Draw(gr);
		}
	}
}
