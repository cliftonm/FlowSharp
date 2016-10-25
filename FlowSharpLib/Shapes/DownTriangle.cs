/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;

namespace FlowSharpLib
{
    public class DownTriangle : GraphicElement
    {
        protected Point[] path;

        public DownTriangle(Canvas canvas) : base(canvas)
        {
        }

        public override List<ConnectionPoint> GetConnectionPoints()
        {
            List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();
            connectionPoints.Add(new ConnectionPoint(GripType.TopMiddle, DisplayRectangle.TopMiddle()));
            connectionPoints.Add(new ConnectionPoint(GripType.BottomMiddle, DisplayRectangle.BottomMiddle()));
            connectionPoints.Add(new ConnectionPoint(GripType.TopLeft, DisplayRectangle.TopLeftCorner()));
            connectionPoints.Add(new ConnectionPoint(GripType.TopRight, DisplayRectangle.TopRightCorner()));

            return connectionPoints;
        }

        public override void UpdatePath()
        {
            path = new Point[]
            {
                new Point(DisplayRectangle.X + DisplayRectangle.Width/2,        DisplayRectangle.Y + DisplayRectangle.Height),        // middle, bottom
                new Point(DisplayRectangle.X + DisplayRectangle.Width,          DisplayRectangle.Y),                              // right, top
                new Point(DisplayRectangle.X,          DisplayRectangle.Y),          // left, top
                new Point(DisplayRectangle.X + DisplayRectangle.Width/2,        DisplayRectangle.Y + DisplayRectangle.Height),        // middle, bottom
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
