/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;

namespace FlowSharpLib
{
    /// <summary>
    /// Left-right dynamic connector.
    /// Routing around shapes is ignored, which means that the best route may include going inside a connected shape.
    /// </summary>
    [ToolboxOrder(11)]
    public class DynamicConnectorLR : DynamicConnector
	{
		public override Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(anchorWidthHeight + 1 + BorderPen.Width); } }

        public DynamicConnectorLR(Canvas canvas) : base(canvas)
		{
			Initialize();
		}

		public DynamicConnectorLR(Canvas canvas, Point start, Point end): base(canvas)
		{
			Initialize();
			StartPoint = start;
			EndPoint = end;
            DisplayRectangle = RecalcDisplayRectangle();
		}

		protected void Initialize()
		{
			lines.Add(new HorizontalLine(canvas));
			lines.Add(new VerticalLine(canvas));
			lines.Add(new HorizontalLine(canvas));
		}

		public override List<ShapeAnchor> GetAnchors()
		{
			Size szAnchor = new Size(anchorWidthHeight, anchorWidthHeight);
            Rectangle vline = GetVerticalLineRectangle();

            return new List<ShapeAnchor>() {
				new ShapeAnchor(GripType.Start, new Rectangle(StartPoint.Move(-anchorWidthHeight/2, -anchorWidthHeight/2), szAnchor), Cursors.Arrow),
				new ShapeAnchor(GripType.End, new Rectangle(EndPoint.Move(-anchorWidthHeight/2, -anchorWidthHeight/2), szAnchor), Cursors.Arrow),
                new ShapeAnchor(GripType.LeftMiddle, new Rectangle(new Point(vline.X + szAnchor.Width, vline.Y + vline.Height/2 - szAnchor.Height/2), szAnchor), Cursors.SizeWE),
            };
		}

		public override GraphicElement CloneDefault(Canvas canvas)
		{
			DynamicConnectorLR line = (DynamicConnectorLR)base.CloneDefault(canvas);
			line.StartCap = StartCap;
			line.EndCap = EndCap;

			return line;
		}

        public override void UpdateSize(ShapeAnchor anchor, Point delta)
        {
            if (anchor.Type == GripType.LeftMiddle)
            {
                vxAdjust += delta.X;
                UpdatePath();
                Rectangle newRect = RecalcDisplayRectangle();
                canvas.Controller.UpdateDisplayRectangle(this, newRect, delta);
            }
            else
            {
                base.UpdateSize(anchor, delta);
            }
        }

        public override void UpdatePath()
		{
            UpdateCaps();

            /*
               ----!
                   !
                   !----
            */

            int xmin = StartPoint.X.Min(EndPoint.X);
            int xmax = StartPoint.X.Max(EndPoint.X);
            int vx = xmin + (xmax - xmin) / 2 + vxAdjust;
            int x1a = StartPoint.X.Min(vx);
            int x1b = StartPoint.X.Max(vx);
            int x2a = EndPoint.X.Min(vx);
            int x2b = EndPoint.X.Max(vx);
            int x1y = StartPoint.Y - BaseController.MIN_HEIGHT / 2;
            int x2y = EndPoint.Y - BaseController.MIN_HEIGHT / 2;
            int vy1 = StartPoint.Y.Min(EndPoint.Y);
            int vy2 = StartPoint.Y.Max(EndPoint.Y);

            lines[0].DisplayRectangle = new Rectangle(x1a, x1y, x1b - x1a, BaseController.MIN_HEIGHT);
            lines[1].DisplayRectangle = new Rectangle(vx - BaseController.MIN_WIDTH / 2, vy1, BaseController.MIN_WIDTH, vy2 - vy1);
            lines[2].DisplayRectangle = new Rectangle(x2a, x2y, x2b - x2a, BaseController.MIN_HEIGHT);

            lines.ForEach(l => l.UpdatePath());
		}

        protected void UpdateCaps()
        {
            if (StartPoint.X < EndPoint.X)
            {
                lines[0].EndCap = AvailableLineCap.None;
                lines[2].StartCap = AvailableLineCap.None;
                lines[0].StartCap = StartCap;
                lines[2].EndCap = EndCap;
            }
            else
            {
                lines[0].StartCap = AvailableLineCap.None;
                lines[2].EndCap = AvailableLineCap.None;
                lines[0].EndCap = StartCap;
                lines[2].StartCap = EndCap;
            }

            lines.ForEach(l => l.UpdateProperties());
        }

        protected Rectangle GetVerticalLineRectangle()
        {
            int xmin = StartPoint.X.Min(EndPoint.X);
            int xmax = StartPoint.X.Max(EndPoint.X);
            int vx = xmin + (xmax - xmin) / 2 + vxAdjust;
            int vy1 = StartPoint.Y.Min(EndPoint.Y);
            int vy2 = StartPoint.Y.Max(EndPoint.Y);

            return new Rectangle(vx - BaseController.MIN_WIDTH / 2, vy1, BaseController.MIN_WIDTH, vy2 - vy1);
        }
    }
}
