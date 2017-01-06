/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Drawing;

namespace FlowSharpLib
{
    [ToolboxOrder(3)]
    public class Diamond : GraphicElement
	{
		protected Point[] path;

		public Diamond(Canvas canvas) : base(canvas)
		{
			HasCornerConnections = false;
		}

		public override void UpdatePath()
		{
			path = new Point[]
			{
				new Point(DisplayRectangle.X,                             DisplayRectangle.Y + DisplayRectangle.Height/2),
				new Point(DisplayRectangle.X + DisplayRectangle.Width/2,		DisplayRectangle.Y),
				new Point(DisplayRectangle.X + DisplayRectangle.Width,    DisplayRectangle.Y + DisplayRectangle.Height/2),
				new Point(DisplayRectangle.X + DisplayRectangle.Width/2,		DisplayRectangle.Y + DisplayRectangle.Height),
			};
		}

		public override void Draw(Graphics gr)
		{
			gr.FillPolygon(FillBrush, path);
			gr.DrawPolygon(BorderPen, path);
			base.Draw(gr);
		}
	}
}
