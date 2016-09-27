using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace FlowSharpLib
{
	/// <summary>
	/// Routing around shapes is ignored, which means that the best route may include going inside a connected shape.
	/// </summary>
	public class DynamicConnector : GraphicElement// , ILine
	{
		public AvailableLineCap StartCap { get; set; }
		public AvailableLineCap EndCap { get; set; }

		public override Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(anchorSize); } }

		protected List<ILine> lines = new List<ILine>();

		protected Point startPoint;
		protected Point endPoint;

		public DynamicConnector(Canvas canvas) : base(canvas)
		{
			HasCornerAnchors = false;
			HasCenterAnchors = false;
			HasTopBottomAnchors = false;
			HasLeftRightAnchors = false;
		}

		public DynamicConnector(Canvas canvas, Point start, Point end): base(canvas)
		{
			// Dummy rectangle for dynamic connector.
			HasCornerAnchors = false;
			HasCenterAnchors = false;
			HasTopBottomAnchors = false;
			HasLeftRightAnchors = false;
			lines.Add(new HorizontalLine(canvas));
			lines.Add(new VerticalLine(canvas));
			lines.Add(new HorizontalLine(canvas));
			startPoint = start;
			endPoint = end;
		}

		public override Rectangle DefaultRectangle()
		{
			return new Rectangle(startPoint, new Size(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y));
		}

		public override List<ShapeAnchor> GetAnchors()
		{
			Size szAnchor = new Size(anchorSize, anchorSize);

			return new List<ShapeAnchor>() {
				new ShapeAnchor(AnchorPosition.Start, new Rectangle(startPoint.Move(0, -anchorSize/2), szAnchor)),
				new ShapeAnchor(AnchorPosition.End, new Rectangle(endPoint.Move(-anchorSize/2, -anchorSize/2), szAnchor)),
			};
		}

		public override ElementProperties CreateProperties()
		{
			return new DynamicConnectorProperties(this);
		}

		public override GraphicElement Clone(Canvas canvas)
		{
			DynamicConnector line = (DynamicConnector)base.Clone(canvas);
			line.StartCap = StartCap;
			line.EndCap = EndCap;

			return line;
		}

		public override void UpdateSize(ShapeAnchor anchor, Point delta)
		{
			if (anchor.Type == AnchorPosition.Start)
			{
				startPoint = startPoint.Move(delta);
				UpdatePath();
			}
			else
			{
				endPoint = endPoint.Move(delta);
				UpdatePath();
			}

			Rectangle newRect = RecalcDisplayRectangle();
			canvas.Controller.UpdateDisplayRectangle(this, newRect, delta);
		}

		public override void UpdatePath()
		{
			// TODO: Figure out whether we're doing H-V-H, or V-H-V, or H-V or V-H, or something even more complicated if we are avoiding shape boundaries.

			lines[0].StartCap = StartCap;
			lines[2].EndCap = EndCap;

			if (startPoint.X < endPoint.X)
			{
				lines[0].DisplayRectangle = new Rectangle(startPoint.X, startPoint.Y - BaseController.MIN_WIDTH / 2, (endPoint.X - startPoint.X) / 2, BaseController.MIN_HEIGHT);
			}
			else
			{
				lines[0].DisplayRectangle = new Rectangle(endPoint.X + (startPoint.X - endPoint.X)/2, startPoint.Y - BaseController.MIN_WIDTH / 2, (startPoint.X - endPoint.X) / 2, BaseController.MIN_HEIGHT);
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
				lines[2].DisplayRectangle = new Rectangle(endPoint.X, endPoint.Y - BaseController.MIN_WIDTH / 2, (startPoint.X - endPoint.X) / 2, BaseController.MIN_HEIGHT);
			}

			lines.ForEach(l => ((GraphicElement)l).UpdatePath());
		}

		protected virtual Rectangle RecalcDisplayRectangle()
		{
			int x1 = startPoint.X.Min(endPoint.X);
			int y1 = startPoint.Y.Min(endPoint.Y);
			int x2 = startPoint.X.Max(endPoint.X);
			int y2 = startPoint.Y.Max(endPoint.Y);

			return new Rectangle(x1, y1, x2 - x1, y2 - y1);
		}

		protected override void Draw(Graphics gr)
		{
			lines.ForEach(l => ((GraphicElement)l).Draw());
			//canvas.Controller.Redraw(hline);
			//canvas.Controller.Redraw(vline);

			// No selection box!
			// base.Draw(gr);
		}
	}
}
