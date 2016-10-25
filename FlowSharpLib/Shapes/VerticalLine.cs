/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Drawing;

namespace FlowSharpLib
{
	public class VerticalLine : Line
	{
		// Fixes background erase issues with dynamic connector with endcaps.
        // + 1 needed for arrows, + 2 needed for diamonds.
		public override Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(anchorWidthHeight + 2 + BorderPen.Width); } }

		public VerticalLine(Canvas canvas) : base(canvas)
		{
			HasCornerAnchors = false;
			HasCenterAnchors = false;
			HasTopBottomAnchors = true;
			HasCornerConnections = false;
			HasCenterConnections = false;
			HasTopBottomConnections = true;
		}

		public override GraphicElement CloneDefault(Canvas canvas)
		{
			VerticalLine line = (VerticalLine)base.CloneDefault(canvas);
			line.StartCap = StartCap;
			line.EndCap = EndCap;

			return line;
		}

		public override Rectangle DefaultRectangle()
		{
			return new Rectangle(20, 20, 20, 40);
		}

        public override void MoveAnchor(ConnectionPoint cpShape, ConnectionPoint cp)
		{
			if (cp.Type == GripType.Start)
			{
				DisplayRectangle = new Rectangle(cpShape.Point.X-BaseController.MIN_WIDTH/2, cpShape.Point.Y, DisplayRectangle.Size.Width, DisplayRectangle.Size.Height);
			}
			else
			{
				DisplayRectangle = new Rectangle(cpShape.Point.X-BaseController.MIN_WIDTH/2, cpShape.Point.Y - DisplayRectangle.Size.Height, DisplayRectangle.Size.Width, DisplayRectangle.Size.Height);
			}

			// TODO: Redraw is updating too much in this case -- causes jerky motion of attached shape.
			// canvas.Controller.Redraw(this, (cpShape.Point.X - cp.Point.X).Abs() + BaseController.MIN_WIDTH, (cpShape.Point.Y - cp.Point.Y).Abs() + BaseController.MIN_HEIGHT);
		}

		public override void Draw(Graphics gr)
		{
			Pen pen = (Pen)BorderPen.Clone();

			if (ShowLineAsSelected)
			{
				pen.Color = pen.Color.ToArgb() == Color.Red.ToArgb() ? Color.Blue : Color.Red;
			}

			gr.DrawLine(pen, DisplayRectangle.TopMiddle(), DisplayRectangle.BottomMiddle());
			pen.Dispose();

			base.Draw(gr);
		}
	}
}
