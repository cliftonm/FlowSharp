/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FlowSharpLib
{
    public class DiagonalConnector : DynamicConnector
    {
        public override Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(anchorWidthHeight + 1 + BorderPen.Width); } }

        public DiagonalConnector(Canvas canvas) : base(canvas)
        {
            Initialize();
        }

        public DiagonalConnector(Canvas canvas, Point start, Point end) : base(canvas)
        {
            startPoint = start;
            endPoint = end;
            DisplayRectangle = RecalcDisplayRectangle();
            Initialize();
        }

        protected void Initialize()
        {
        }

        public override bool IsSelectable(Point p)
        {
            bool ret = false;
            // Issue #30
            // Determine if point is near line, rather than whether the point is inside the update rectangle.
            // See: http://stackoverflow.com/questions/910882/how-can-i-tell-if-a-point-is-nearby-a-certain-line

            // First qualify by the point being inside the update rectangle itself.
            if (UpdateRectangle.Contains(p))
            {
                // Then check how close the point is.
                int a = p.X - UpdateRectangle.X;
                int b = p.Y - UpdateRectangle.Y;
                int c = UpdateRectangle.Width;
                int d = UpdateRectangle.Height;

                int dist = (int)(Math.Abs(a * d - c * b) / Math.Sqrt(c * c + d * d));
                ret = dist <= BaseController.MIN_HEIGHT;
            }

            return ret;
        }

        public override List<ShapeAnchor> GetAnchors()
        {
            Size szAnchor = new Size(anchorWidthHeight, anchorWidthHeight);

            int startxOffset = startPoint.X < endPoint.X ? 0 : -anchorWidthHeight;
            int startyOffset = startPoint.Y < endPoint.Y ? 0: -anchorWidthHeight;

            int endxOffset = startPoint.X < endPoint.X ? -anchorWidthHeight : 0;
            int endyOffset = startPoint.Y < endPoint.Y ? -anchorWidthHeight : 0;

            return new List<ShapeAnchor>() {
                new ShapeAnchor(GripType.Start, new Rectangle(startPoint.Move(startxOffset/2, startyOffset/2), szAnchor), Cursors.Arrow),
                new ShapeAnchor(GripType.End, new Rectangle(endPoint.Move(endxOffset/2, endyOffset/2), szAnchor), Cursors.Arrow),
            };
        }

        public override GraphicElement CloneDefault(Canvas canvas)
        {
            DiagonalConnector dc = (DiagonalConnector)base.CloneDefault(canvas);
            dc.StartCap = StartCap;
            dc.EndCap = EndCap;

            return dc;
        }

        public override void UpdateProperties()
        {
            if (StartCap == AvailableLineCap.None)
            {
                BorderPen.StartCap = LineCap.NoAnchor;
            }
            else
            {
                BorderPen.CustomStartCap = StartCap == AvailableLineCap.Arrow ? adjCapArrow : adjCapDiamond;
            }

            if (EndCap == AvailableLineCap.None)
            {
                BorderPen.EndCap = LineCap.NoAnchor;
            }
            else
            {
                BorderPen.CustomEndCap = EndCap == AvailableLineCap.Arrow ? adjCapArrow : adjCapDiamond;
            }

            base.UpdateProperties();
        }

        // TODO: Clean this up, and the GraphicElement.Internal... methods, as this is an ugly workaround to 
        // needing to access the GraphicElement base class methods since they're overridden by the DynamicConnector
        // class, which also uses a lines[] list, which a diagonal connector doesn't have.  The result points
        // to bad class design in the hierarchy, which needs to be fixed.

        public override void GetBackground()
        {
            InternalGetBackground();
        }

        public override void CancelBackground()
        {
            InternalCancelBackground();
        }

        public override void Erase()
        {
            InternalErase();
        }

        public override void UpdateScreen(int ix = 0, int iy = 0)
        {
            InternalUpdateScreen(ix, iy);
        }

        public override void Draw(Graphics gr)
        {
            Pen pen = (Pen)BorderPen.Clone();

            if (ShowConnectorAsSelected || Selected)
            {
                pen.Color = pen.Color.ToArgb() == Color.Red.ToArgb() ? Color.Blue : Color.Red;
            }

            gr.DrawLine(pen, startPoint, endPoint);
            pen.Dispose();

            base.Draw(gr);
        }
    }
}
