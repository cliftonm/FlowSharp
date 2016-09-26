using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FlowSharpLib
{
	/// <summary>
	/// Currently, this is a "poor-man's" connector, consisting of only (at most) two lines at right angles.
	/// Routing around shapes is ignored.
	/// </summary>
	public class DynamicConnector : GraphicElement, ILine
	{
		public AvailableLineCap StartCap { get; set; }
		public AvailableLineCap EndCap { get; set; }

		public DynamicConnector(Canvas canvas) : base(canvas)
		{
			FillBrush = new SolidBrush(Color.White);
			BorderPen = new Pen(Color.Black);
			BorderPen.Width = 1;
			HasCornerAnchors = false;
			HasCenterAnchors = false;
			HasTopBottomAnchors = true;
		}

		public override ElementProperties CreateProperties()
		{
			return new LineProperties(this);
		}

		public override GraphicElement Clone(Canvas canvas)
		{
			DynamicConnector line = (DynamicConnector)base.Clone(canvas);
			line.StartCap = StartCap;
			line.EndCap = EndCap;

			return line;
		}

		protected override void Draw(Graphics gr)
		{
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

			gr.DrawLine(pen, DisplayRectangle.TopMiddle(), DisplayRectangle.BottomMiddle());
			pen.Dispose();

			base.Draw(gr);
		}
	}
}
