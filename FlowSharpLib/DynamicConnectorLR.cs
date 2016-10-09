/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;

namespace FlowSharpLib
{
	/// <summary>
	/// Left-right dynamic connector.
	/// Routing around shapes is ignored, which means that the best route may include going inside a connected shape.
	/// </summary>
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
			startPoint = start;
			endPoint = end;
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

			int startxOffset = startPoint.X < endPoint.X ? 0 : -anchorWidthHeight;
			int endxOffset = startPoint.X < endPoint.X ? -anchorWidthHeight : 0;

			return new List<ShapeAnchor>() {
				new ShapeAnchor(GripType.Start, new Rectangle(startPoint.Move(startxOffset, -anchorWidthHeight/2), szAnchor)),
				new ShapeAnchor(GripType.End, new Rectangle(endPoint.Move(endxOffset, -anchorWidthHeight/2), szAnchor)),
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

            if (startPoint.X < endPoint.X)
			{
				lines[0].DisplayRectangle = new Rectangle(startPoint.X, startPoint.Y - BaseController.MIN_HEIGHT / 2, (endPoint.X - startPoint.X) / 2, BaseController.MIN_HEIGHT);
			}
            else
            {
                lines[0].DisplayRectangle = new Rectangle(endPoint.X + (startPoint.X - endPoint.X) / 2, startPoint.Y - BaseController.MIN_HEIGHT / 2, (startPoint.X - endPoint.X) / 2, BaseController.MIN_HEIGHT);
            }

            if (startPoint.Y < endPoint.Y)
			{
				lines[1].DisplayRectangle = new Rectangle(startPoint.X + (endPoint.X - startPoint.X) / 2 - BaseController.MIN_WIDTH / 2, startPoint.Y, BaseController.MIN_WIDTH, endPoint.Y - startPoint.Y);
			}
			else
			{
				lines[1].DisplayRectangle = new Rectangle(endPoint.X + (startPoint.X - endPoint.X) / 2 - BaseController.MIN_WIDTH / 2, endPoint.Y, BaseController.MIN_WIDTH, startPoint.Y - endPoint.Y);
			}

			if (startPoint.X < endPoint.X)
			{
				lines[2].DisplayRectangle = new Rectangle(startPoint.X + (endPoint.X - startPoint.X) / 2, endPoint.Y - BaseController.MIN_HEIGHT / 2, (endPoint.X - startPoint.X) / 2, BaseController.MIN_HEIGHT);
			}
			else
			{
				lines[2].DisplayRectangle = new Rectangle(endPoint.X, endPoint.Y - BaseController.MIN_HEIGHT / 2, (startPoint.X - endPoint.X) / 2, BaseController.MIN_HEIGHT);
			}

            lines.ForEach(l => l.UpdatePath());
		}

        protected void UpdateCaps()
        {
            if (startPoint.X < endPoint.X)
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
