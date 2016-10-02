/* The MIT License (MIT)
* 
* Copyright (c) 2016 Marc Clifton
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System.Drawing;
using System.Drawing.Drawing2D;

namespace FlowSharpLib
{
	public enum AvailableLineCap
	{
		None,
		Arrow,
        Diamond,
	};

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

			if (ShowLineAsSelected)
			{
				pen.Color = pen.Color.ToArgb() == Color.Red.ToArgb() ? Color.Blue : Color.Red;
			}

            gr.DrawLine(pen, DisplayRectangle.LeftMiddle(), DisplayRectangle.RightMiddle());
			pen.Dispose();

			base.Draw(gr);
		}
	}
}
