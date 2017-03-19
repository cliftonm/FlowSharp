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
    [ToolboxOrder(6)]
    public class UpTriangle : GraphicElement
    {
        protected Point[] path;

        public UpTriangle(Canvas canvas) : base(canvas)
        {
        }

        public override List<ConnectionPoint> GetConnectionPoints()
        {
            List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();
            connectionPoints.Add(new ConnectionPoint(GripType.TopMiddle, ZoomRectangle.TopMiddle()));
            connectionPoints.Add(new ConnectionPoint(GripType.BottomMiddle, ZoomRectangle.BottomMiddle()));
            connectionPoints.Add(new ConnectionPoint(GripType.BottomLeft, ZoomRectangle.BottomLeftCorner()));
            connectionPoints.Add(new ConnectionPoint(GripType.BottomRight, ZoomRectangle.BottomRightCorner()));

            return connectionPoints;
        }

        public override void UpdatePath()
        {
            path = new Point[]
            {
                new Point(ZoomRectangle.X + ZoomRectangle.Width/2,        ZoomRectangle.Y),        // middle, top
                new Point(ZoomRectangle.X + ZoomRectangle.Width,          ZoomRectangle.Y + ZoomRectangle.Height),                              // right, bottom
                new Point(ZoomRectangle.X,          ZoomRectangle.Y + ZoomRectangle.Height),          // left, bottom
                new Point(ZoomRectangle.X + ZoomRectangle.Width/2,        ZoomRectangle.Y),        // middle, Top
            };
        }

        protected Point[] ZPath()
        {
            Rectangle r = ZoomRectangle;
            r.X = 0;
            r.Y = 0;
            int adjust = (int)((BorderPen.Width + 0) / 2);
            Point[] path = new Point[]
            {
                new Point(r.X + r.Width/2, r.Y + adjust),        // right, middle
                new Point(r.X + r.Width - adjust,           r.Y + r.Height - adjust),                              // left, top
                new Point(r.X + adjust,           r.Y + r.Height - adjust),          // left, bottom
                new Point(r.X + r.Width/2, r.Y + adjust),        // right, middle
            };

            return path;
        }

        public override void Draw(Graphics gr, bool showSelection = true)
        {
            Rectangle r = ZoomRectangle.Grow(2);
            Bitmap bitmap = new Bitmap(r.Width, r.Height);
            Graphics g2 = Graphics.FromImage(bitmap);
            g2.SmoothingMode = SmoothingMode.AntiAlias;
            Point[] path = ZPath();
            g2.FillPolygon(FillBrush, path);
            g2.DrawPolygon(BorderPen, path);
            gr.DrawImage(bitmap, ZoomRectangle.X, ZoomRectangle.Y);
            bitmap.Dispose();
            g2.Dispose();
            base.Draw(gr, showSelection);
        }
    }
}
