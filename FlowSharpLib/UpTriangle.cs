/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;

namespace FlowSharpLib
{
    public class UpTriangle : GraphicElement
    {
        protected Point[] path;

        public UpTriangle(Canvas canvas) : base(canvas)
        {
        }

        public override List<ConnectionPoint> GetConnectionPoints()
        {
            List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();
            connectionPoints.Add(new ConnectionPoint(GripType.Start, DisplayRectangle.TopMiddle()));
            connectionPoints.Add(new ConnectionPoint(GripType.End, DisplayRectangle.BottomMiddle()));
            connectionPoints.Add(new ConnectionPoint(GripType.Start, DisplayRectangle.BottomLeftCorner()));
            connectionPoints.Add(new ConnectionPoint(GripType.End, DisplayRectangle.BottomRightCorner()));

            return connectionPoints;
        }

        public override void UpdatePath()
        {
            path = new Point[]
            {
                new Point(DisplayRectangle.X + DisplayRectangle.Width/2,        DisplayRectangle.Y),        // middle, top
                new Point(DisplayRectangle.X + DisplayRectangle.Width,          DisplayRectangle.Y + DisplayRectangle.Height),                              // right, bottom
                new Point(DisplayRectangle.X,          DisplayRectangle.Y + DisplayRectangle.Height),          // left, bottom
                new Point(DisplayRectangle.X + DisplayRectangle.Width/2,        DisplayRectangle.Y),        // middle, Top
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
