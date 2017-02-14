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
    /// Up-Down dynamic connector.
    /// Routing around shapes is ignored, which means that the best route may include going inside a connected shape.
    /// </summary>
    [ToolboxOrder(12)]
    public class DynamicConnectorUD : DynamicConnector
	{
		public override Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(anchorWidthHeight + 1 + BorderPen.Width); } }

		public DynamicConnectorUD(Canvas canvas) : base(canvas)
		{
			Initialize();
		}

		public DynamicConnectorUD(Canvas canvas, Point start, Point end) : base(canvas)
		{
			Initialize();
			StartPoint = start;
			EndPoint = end;
            DisplayRectangle = RecalcDisplayRectangle();
        }

        protected void Initialize()
		{
			lines.Add(new VerticalLine(canvas));
			lines.Add(new HorizontalLine(canvas));
			lines.Add(new VerticalLine(canvas));
		}

		public override List<ShapeAnchor> GetAnchors()
		{
			Size szAnchor = new Size(anchorWidthHeight, anchorWidthHeight);
            Rectangle hline = GetHorizontalLineRectangle();

            return new List<ShapeAnchor>() {
				new ShapeAnchor(GripType.Start, new Rectangle(StartPoint.Move(-anchorWidthHeight/2, -anchorWidthHeight/2), szAnchor), Cursors.Arrow),
				new ShapeAnchor(GripType.End, new Rectangle(EndPoint.Move(-anchorWidthHeight/2, -anchorWidthHeight/2), szAnchor), Cursors.Arrow),
                new ShapeAnchor(GripType.TopMiddle, new Rectangle(new Point(hline.X + hline.Width/2 - szAnchor.Width/2, hline.Y + szAnchor.Height), szAnchor), Cursors.SizeNS),
            };
		}

		public override GraphicElement CloneDefault(Canvas canvas)
		{
			DynamicConnectorUD line = (DynamicConnectorUD)base.CloneDefault(canvas);
			line.StartCap = StartCap;
			line.EndCap = EndCap;

			return line;
		}

        public override void UpdateSize(ShapeAnchor anchor, Point delta)
        {
            if (anchor.Type == GripType.TopMiddle)
            {
                hyAdjust += delta.Y;
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
                !
                !
                !----!
                     !
                     !
            */

            int ymin = StartPoint.Y.Min(EndPoint.Y);
            int ymax = StartPoint.Y.Max(EndPoint.Y);
            int hy = ymin + (ymax - ymin) / 2 + hyAdjust;
            int y1a = StartPoint.Y.Min(hy);
            int y1b = StartPoint.Y.Max(hy);
            int y2a = EndPoint.Y.Min(hy);
            int y2b = EndPoint.Y.Max(hy);
            int y1x = StartPoint.X - BaseController.MIN_HEIGHT / 2;
            int y2x = EndPoint.X - BaseController.MIN_HEIGHT / 2;
            int hx1 = StartPoint.X.Min(EndPoint.X);
            int hx2 = StartPoint.X.Max(EndPoint.X);

            lines[0].DisplayRectangle = new Rectangle(y1x, y1a, BaseController.MIN_WIDTH, y1b - y1a);
            lines[1].DisplayRectangle = new Rectangle(hx1, hy - BaseController.MIN_HEIGHT / 2, hx2 - hx1, BaseController.MIN_HEIGHT);
            lines[2].DisplayRectangle = new Rectangle(y2x, y2a, BaseController.MIN_WIDTH, y2b - y2a);

            lines.ForEach(l => l.UpdatePath());
		}

        protected void UpdateCaps()
        {
            if (StartPoint.Y < EndPoint.Y)
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

        protected Rectangle GetHorizontalLineRectangle()
        {
            int ymin = StartPoint.Y.Min(EndPoint.Y);
            int ymax = StartPoint.Y.Max(EndPoint.Y);
            int hy = ymin + (ymax - ymin) / 2 + hyAdjust;
            int hx1 = StartPoint.X.Min(EndPoint.X);
            int hx2 = StartPoint.X.Max(EndPoint.X);

            return new Rectangle(hx1, hy - BaseController.MIN_HEIGHT / 2, hx2 - hx1, BaseController.MIN_HEIGHT);
        }
    }
}
