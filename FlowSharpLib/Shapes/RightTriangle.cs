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
    public class RightTriangle : GraphicElement
    {
        protected Point[] path;

        public RightTriangle(Canvas canvas) : base(canvas)
        {
        }

        public override List<ConnectionPoint> GetConnectionPoints()
        {
            List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();
            connectionPoints.Add(new ConnectionPoint(GripType.LeftMiddle, DisplayRectangle.LeftMiddle()));
            connectionPoints.Add(new ConnectionPoint(GripType.RightMiddle, DisplayRectangle.RightMiddle()));
            connectionPoints.Add(new ConnectionPoint(GripType.TopLeft, DisplayRectangle.TopLeftCorner()));
            connectionPoints.Add(new ConnectionPoint(GripType.BottomLeft, DisplayRectangle.BottomLeftCorner()));

            return connectionPoints;
        }

        public override void UpdatePath()
        {
            Rectangle r = DisplayRectangle;
            path = new Point[]
            {
                new Point(r.X + r.Width,          r.Y + r.Height/2),        // right, middle
                new Point(r.X,          r.Y),                              // left, top
                new Point(r.X,          r.Y + r.Height),          // left, bottom
                new Point(r.X + r.Width,                             r.Y + r.Height/2),        // right, middle
            };
        }

        protected Point[] ZPath()
        {
            Rectangle r = DisplayRectangle;
            r.X = 0;
            r.Y = 0;
            Point[] path = new Point[]
            {
                new Point(r.X + r.Width, r.Y + r.Height/2),        // right, middle
                new Point(r.X,           r.Y),                              // left, top
                new Point(r.X,           r.Y + r.Height),          // left, bottom
                new Point(r.X + r.Width, r.Y + r.Height/2),        // right, middle
            };

            return path;
        }

        public override void Draw(Graphics gr)
        {
            // While this clips the region, the lines are no longer antialiased.
            /*
            GraphicsPath gp = new GraphicsPath();
            gp.AddPolygon(path);
            Region region = new Region(gp);
            gr.SetClip(region, CombineMode.Replace);
            gr.IntersectClip(DisplayRectangle);
            ...
            gr.ResetClip();
            */

            // Drawing onto a bitmap that constrains the drawing area fixes the trail problem
            // but still has issues with larger pen widths (try 10) as triangle points are clipped.
            Rectangle r = DisplayRectangle;
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
