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
    [ToolboxOrder(7)]
    public class DownTriangle : GraphicElement
    {
        protected Point[] path;

        public DownTriangle(Canvas canvas) : base(canvas)
        {
        }

        public override List<ConnectionPoint> GetConnectionPoints()
        {
            List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();
            connectionPoints.Add(new ConnectionPoint(GripType.TopMiddle, ZoomRectangle.TopMiddle()));
            connectionPoints.Add(new ConnectionPoint(GripType.BottomMiddle, ZoomRectangle.BottomMiddle()));
            connectionPoints.Add(new ConnectionPoint(GripType.TopLeft, ZoomRectangle.TopLeftCorner()));
            connectionPoints.Add(new ConnectionPoint(GripType.TopRight, ZoomRectangle.TopRightCorner()));

            return connectionPoints;
        }

        public override void UpdatePath()
        {
            path = new Point[]
            {
                new Point(ZoomRectangle.X + ZoomRectangle.Width/2,        ZoomRectangle.Y + ZoomRectangle.Height),        // middle, bottom
                new Point(ZoomRectangle.X + ZoomRectangle.Width,          ZoomRectangle.Y),                              // right, top
                new Point(ZoomRectangle.X,          ZoomRectangle.Y),          // left, top
                new Point(ZoomRectangle.X + ZoomRectangle.Width/2,        ZoomRectangle.Y + ZoomRectangle.Height),        // middle, bottom
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
                new Point(r.X + r.Width/2,          r.Y + r.Height - adjust),
                new Point(r.X + r.Width - adjust,   r.Y + adjust),
                new Point(r.X + adjust,             r.Y + adjust),          
                new Point(r.X + r.Width/2,          r.Y + r.Height - adjust),
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
