/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FlowSharpLib
{
	/// <summary>
	/// Left-down dynamic connector. (horizontal line, vertical line at right.)
	/// Routing around shapes is ignored, which means that the best route may include going inside a connected shape.
	/// </summary>
	public class DynamicConnectorLD : DynamicConnector
	{
		public override Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(anchorWidthHeight + 1 + BorderPen.Width); } }

		public DynamicConnectorLD(Canvas canvas) : base(canvas)
		{
			Initialize();
		}

		public DynamicConnectorLD(Canvas canvas, Point start, Point end) : base(canvas)
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
		}

		public override List<ShapeAnchor> GetAnchors()
		{
			Size szAnchor = new Size(anchorWidthHeight, anchorWidthHeight);

			int startxOffset = StartPoint.X < EndPoint.X ? 0 : -anchorWidthHeight;
			int endyOffset = StartPoint.Y < EndPoint.Y ? -anchorWidthHeight : 0;

			return new List<ShapeAnchor>() {
				new ShapeAnchor(GripType.Start, new Rectangle(StartPoint.Move(startxOffset, -anchorWidthHeight/2), szAnchor), Cursors.Arrow),
				new ShapeAnchor(GripType.End, new Rectangle(EndPoint.Move(-anchorWidthHeight/2, endyOffset), szAnchor), Cursors.Arrow),
			};
		}

		public override GraphicElement CloneDefault(Canvas canvas)
		{
			DynamicConnectorLD line = (DynamicConnectorLD)base.CloneDefault(canvas);
			line.StartCap = StartCap;
			line.EndCap = EndCap;

			return line;
		}

		public override void UpdatePath()
		{
            UpdateCaps();

            if (StartPoint.X < EndPoint.X)
			{
				lines[0].DisplayRectangle = new Rectangle(StartPoint.X, StartPoint.Y - BaseController.MIN_HEIGHT / 2, EndPoint.X - StartPoint.X, BaseController.MIN_HEIGHT);
			}
			else
			{
				lines[0].DisplayRectangle = new Rectangle(EndPoint.X, StartPoint.Y - BaseController.MIN_HEIGHT / 2, StartPoint.X - EndPoint.X, BaseController.MIN_HEIGHT);
			}

			if (StartPoint.Y < EndPoint.Y)
			{
				lines[1].DisplayRectangle = new Rectangle(EndPoint.X - BaseController.MIN_WIDTH / 2, StartPoint.Y, BaseController.MIN_WIDTH, EndPoint.Y - StartPoint.Y);
			}
			else
			{
				lines[1].DisplayRectangle = new Rectangle(EndPoint.X - BaseController.MIN_WIDTH / 2, EndPoint.Y, BaseController.MIN_WIDTH, StartPoint.Y - EndPoint.Y);
			}

			lines.ForEach(l => l.UpdatePath());
		}

        protected void UpdateCaps()
        {
            if (StartPoint.X < EndPoint.X)
            {
                lines[0].StartCap = StartCap;
                lines[0].EndCap = AvailableLineCap.None;
            }
            else
            {
                lines[0].StartCap = AvailableLineCap.None;
                lines[0].EndCap = StartCap;
            }

            if (StartPoint.Y < EndPoint.Y)
            {
                lines[1].StartCap = AvailableLineCap.None;
                lines[1].EndCap = EndCap;
            }
            else
            {
                lines[1].StartCap = EndCap;
                lines[1].EndCap = AvailableLineCap.None;
            }

            lines.ForEach(l => l.UpdateProperties());
        }
    }
}
