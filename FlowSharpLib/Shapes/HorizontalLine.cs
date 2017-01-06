/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Drawing;

namespace FlowSharpLib
{
	public enum AvailableLineCap
	{
		None,
		Arrow,
        Diamond,
	};

    [ToolboxOrder(8)]
    public class HorizontalLine : Line
	{
		// Fixes background erase issues with dynamic connector.
		public override Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(anchorWidthHeight + 2 + BorderPen.Width); } }

		public HorizontalLine(Canvas canvas) : base(canvas)
		{
			HasCornerAnchors = false;
			HasCenterAnchors = false;
			HasLeftRightAnchors = true;
			HasCornerConnections = false;
			HasCenterConnections = false;
			HasLeftRightConnections = true;
		}

		public override GraphicElement CloneDefault(Canvas canvas)
		{
			HorizontalLine line = (HorizontalLine)base.CloneDefault(canvas);
			line.StartCap = StartCap;
			line.EndCap = EndCap;

			return line;
		}

		public override Rectangle DefaultRectangle()
		{
			return new Rectangle(20, 20, 40, 20);
		}

        public override void MoveAnchor(ConnectionPoint cpShape, ConnectionPoint cp)
		{
			if (cp.Type == GripType.Start)
			{
				DisplayRectangle = new Rectangle(cpShape.Point.X, cpShape.Point.Y -BaseController.MIN_HEIGHT/2, DisplayRectangle.Size.Width, DisplayRectangle.Size.Height);
			}
			else
			{
				DisplayRectangle = new Rectangle(cpShape.Point.X-DisplayRectangle.Size.Width, cpShape.Point.Y - BaseController.MIN_HEIGHT/2, DisplayRectangle.Size.Width, DisplayRectangle.Size.Height);
			}

			// TODO: Redraw is updating too much in this case -- causes jerky motion of attached shape.
			canvas.Controller.Redraw(this, (cpShape.Point.X - cp.Point.X).Abs() + BaseController.MIN_WIDTH, (cpShape.Point.Y - cp.Point.Y).Abs() + BaseController.MIN_HEIGHT);
		}

		public override void Draw(Graphics gr)
		{
			Pen pen = (Pen)BorderPen.Clone();

			if (ShowConnectorAsSelected)
			{
				pen.Color = pen.Color.ToArgb() == Color.Red.ToArgb() ? Color.Blue : Color.Red;
			}

            gr.DrawLine(pen, DisplayRectangle.LeftMiddle(), DisplayRectangle.RightMiddle());
			pen.Dispose();

			base.Draw(gr);
		}
	}
}
