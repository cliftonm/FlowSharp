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

			return new List<ShapeAnchor>() {
				new ShapeAnchor(GripType.Start, new Rectangle(StartPoint.Move(-anchorWidthHeight/2, -anchorWidthHeight/2), szAnchor), Cursors.Arrow),
				new ShapeAnchor(GripType.End, new Rectangle(EndPoint.Move(-anchorWidthHeight/2, -anchorWidthHeight/2), szAnchor), Cursors.Arrow),
			};
		}

		public override GraphicElement CloneDefault(Canvas canvas)
		{
			DynamicConnectorLR line = (DynamicConnectorLR)base.CloneDefault(canvas);
			line.StartCap = StartCap;
			line.EndCap = EndCap;

			return line;
		}

		public override void UpdatePath()
		{
            UpdateCaps();

            if (StartPoint.X < EndPoint.X)
			{
				lines[0].DisplayRectangle = new Rectangle(StartPoint.X, StartPoint.Y - BaseController.MIN_HEIGHT / 2, (EndPoint.X - StartPoint.X) / 2, BaseController.MIN_HEIGHT);
			}
            else
            {
                lines[0].DisplayRectangle = new Rectangle(EndPoint.X + (StartPoint.X - EndPoint.X) / 2, StartPoint.Y - BaseController.MIN_HEIGHT / 2, (StartPoint.X - EndPoint.X) / 2, BaseController.MIN_HEIGHT);
            }

            if (StartPoint.Y < EndPoint.Y)
			{
				lines[1].DisplayRectangle = new Rectangle(StartPoint.X + (EndPoint.X - StartPoint.X) / 2 - BaseController.MIN_WIDTH / 2, StartPoint.Y, BaseController.MIN_WIDTH, EndPoint.Y - StartPoint.Y);
			}
			else
			{
				lines[1].DisplayRectangle = new Rectangle(EndPoint.X + (StartPoint.X - EndPoint.X) / 2 - BaseController.MIN_WIDTH / 2, EndPoint.Y, BaseController.MIN_WIDTH, StartPoint.Y - EndPoint.Y);
			}

			if (StartPoint.X < EndPoint.X)
			{
				lines[2].DisplayRectangle = new Rectangle(StartPoint.X + (EndPoint.X - StartPoint.X) / 2, EndPoint.Y - BaseController.MIN_HEIGHT / 2, (EndPoint.X - StartPoint.X) / 2, BaseController.MIN_HEIGHT);
			}
			else
			{
				lines[2].DisplayRectangle = new Rectangle(EndPoint.X, EndPoint.Y - BaseController.MIN_HEIGHT / 2, (StartPoint.X - EndPoint.X) / 2, BaseController.MIN_HEIGHT);
			}

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
    }
}
