using System.Drawing;

namespace FlowSharp
{
	public class Diamond : GraphicElement
	{
		protected Point[] path;

		public Diamond(Canvas canvas) : base(canvas)
		{
			FillBrush = new SolidBrush(Color.White);
			BorderPen = new Pen(Color.Purple);
			BorderPen.Width = 1;
			// HasCornerAnchors = false;
		}

		public override void UpdatePath()
		{
			// Path can exceed our UpdateRectangle, so adjust size by pen width / 2.
			// This is particularly a problem with large border pens and TODO: has not been fully resolved.
			int bpw = (int)(BorderPen.Width / 2);
			path = new Point[]
			{
				new Point(DisplayRectangle.X + bpw,                             DisplayRectangle.Y + DisplayRectangle.Height/2),
				new Point(DisplayRectangle.X + DisplayRectangle.Width/2,		DisplayRectangle.Y + bpw),
				new Point(DisplayRectangle.X + DisplayRectangle.Width - bpw,    DisplayRectangle.Y + DisplayRectangle.Height/2),
				new Point(DisplayRectangle.X + DisplayRectangle.Width/2,		DisplayRectangle.Y + DisplayRectangle.Height - bpw),
			};
		}

		protected override void Draw(Graphics gr)
		{
			gr.FillPolygon(FillBrush, path);
			gr.DrawPolygon(BorderPen, path);
			base.Draw(gr);
		}
	}
}
