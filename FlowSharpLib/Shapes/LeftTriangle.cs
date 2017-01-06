/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FlowSharpLib
{
    [ToolboxOrder(4)]
    public class LeftTriangle : GraphicElement
    {
        protected Point[] path;

        public LeftTriangle(Canvas canvas) : base(canvas)
        {
        }

        public override List<ConnectionPoint> GetConnectionPoints()
        {
            List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();
            connectionPoints.Add(new ConnectionPoint(GripType.LeftMiddle, DisplayRectangle.LeftMiddle()));
            connectionPoints.Add(new ConnectionPoint(GripType.RightMiddle, DisplayRectangle.RightMiddle()));
            connectionPoints.Add(new ConnectionPoint(GripType.TopRight, DisplayRectangle.TopRightCorner()));
            connectionPoints.Add(new ConnectionPoint(GripType.BottomRight, DisplayRectangle.BottomRightCorner()));

            return connectionPoints;
        }

        public override void UpdatePath()
        {
            path = new Point[]
            {
                new Point(DisplayRectangle.X,                             DisplayRectangle.Y + DisplayRectangle.Height/2),        // left, middle
                new Point(DisplayRectangle.X + DisplayRectangle.Width,          DisplayRectangle.Y),                              // right, top
                new Point(DisplayRectangle.X + DisplayRectangle.Width,          DisplayRectangle.Y + DisplayRectangle.Height),          // right, bottom
                new Point(DisplayRectangle.X,                             DisplayRectangle.Y + DisplayRectangle.Height/2),        // left, middle
            };
        }

        protected Point[] ZPath()
        {
            Rectangle r = DisplayRectangle;
            r.X = 0;
            r.Y = 0;
            int adjust = (int)((BorderPen.Width + 0) / 2);
            Point[] path = new Point[]
            {
                new Point(r.X + adjust,           r.Y + r.Height/2),
                new Point(r.X + r.Width - adjust, r.Y + adjust),
                new Point(r.X + r.Width - adjust, r.Y + r.Height - adjust),
                new Point(r.X + adjust,           r.Y + r.Height/2),
            };

            return path;
        }

        public override void Draw(Graphics gr)
        {
            Rectangle r = DisplayRectangle.Grow(2);
            Bitmap bitmap = new Bitmap(r.Width, r.Height);
            Graphics g2 = Graphics.FromImage(bitmap);
            g2.SmoothingMode = SmoothingMode.AntiAlias;
            Point[] path = ZPath();
            g2.FillPolygon(FillBrush, path);
            g2.DrawPolygon(BorderPen, path);
            gr.DrawImage(bitmap, DisplayRectangle.X, DisplayRectangle.Y);
            bitmap.Dispose();
            g2.Dispose();
            base.Draw(gr);
        }
    }
}
