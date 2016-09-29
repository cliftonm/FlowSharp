using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace FlowSharpLib
{
	/// <summary>
	/// Left-down dynamic connector. (horizontal line, vertical line at right.)
	/// Routing around shapes is ignored, which means that the best route may include going inside a connected shape.
	/// </summary>
	public class DynamicConnectorLD : DynamicConnector
	{
		public override Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(anchorSize + 1 + BorderPen.Width); } }

		public DynamicConnectorLD(Canvas canvas) : base(canvas)
		{
			Initialize();
		}

		public DynamicConnectorLD(Canvas canvas, Point start, Point end) : base(canvas)
		{
			Initialize();
			startPoint = start;
			endPoint = end;
		}

		protected void Initialize()
		{
			lines.Add(new HorizontalLine(canvas));
			lines.Add(new VerticalLine(canvas));
		}

		public override bool IsSelectable(Point p)
		{
			return lines.Any(l => l.IsSelectable(p));
		}

		public override Rectangle DefaultRectangle()
		{
			return new Rectangle(startPoint, new Size(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y));
		}

		public override List<ShapeAnchor> GetAnchors()
		{
			Size szAnchor = new Size(anchorSize, anchorSize);

			int startxOffset = startPoint.X < endPoint.X ? 0 : -anchorSize;
			int endyOffset = startPoint.Y < endPoint.Y ? -anchorSize : 0;

			return new List<ShapeAnchor>() {
				new ShapeAnchor(GripType.Start, new Rectangle(startPoint.Move(startxOffset, -anchorSize/2), szAnchor)),
				new ShapeAnchor(GripType.End, new Rectangle(endPoint.Move(-anchorSize/2, endyOffset), szAnchor)),
			};
		}

		public override List<ConnectionPoint> GetConnectionPoints()
		{
			return new List<ConnectionPoint>() {
				new ConnectionPoint(GripType.Start, startPoint),
				new ConnectionPoint(GripType.End, endPoint),
			};
		}

		public override GraphicElement Clone(Canvas canvas)
		{
			DynamicConnectorLR line = (DynamicConnectorLR)base.Clone(canvas);
			line.StartCap = StartCap;
			line.EndCap = EndCap;

			return line;
		}

		/// <summary>
		/// Custom move operation of start/end points.
		/// </summary>
		public override void Move(Point delta)
		{
			startPoint = startPoint.Move(delta);
			endPoint = endPoint.Move(delta);
			DisplayRectangle = RecalcDisplayRectangle();
		}

		public override void MoveAnchor(ConnectionPoint cpShape, ConnectionPoint cp)
		{
			if (cp.Type == GripType.Start)
			{
				startPoint = new Point(cpShape.Point.X, cpShape.Point.Y);
			}
			else
			{
				endPoint = new Point(cpShape.Point.X, cpShape.Point.Y);
			}

			UpdatePath();
			DisplayRectangle = RecalcDisplayRectangle();
		}

		public override void MoveAnchor(GripType type, Point delta)
		{
			if (type == GripType.Start)
			{
				startPoint = startPoint.Move(delta);
			}
			else
			{
				endPoint = endPoint.Move(delta);
			}

			UpdatePath();
			DisplayRectangle = RecalcDisplayRectangle();
		}

		public override void UpdateSize(ShapeAnchor anchor, Point delta)
		{
			if (anchor.Type == GripType.Start)
			{
				startPoint = startPoint.Move(delta);
			}
			else
			{
				endPoint = endPoint.Move(delta);
			}

			UpdatePath();
			Rectangle newRect = RecalcDisplayRectangle();
			canvas.Controller.UpdateDisplayRectangle(this, newRect, delta);
		}

		// *** Override all dynamic connector drawing so that the backgrounds are optimized to the line segments, not the entire region. ***

		public override void GetBackground()
		{
			lines.ForEach(l => l.GetBackground());
		}

		public override void CancelBackground()
		{
			lines.ForEach(l => l.CancelBackground());
		}

		public override void Erase()
		{
			// Is reversing necessary?
			lines.AsEnumerable().Reverse().ForEach(l => l.Erase());
		}

		public override void UpdateScreen(int ix = 0, int iy = 0)
		{
			lines.ForEach(l => l.UpdateScreen(ix, iy));
		}

		protected override void Draw(Graphics gr)
		{
			lines.ForEach(l => l.Draw());
			base.Draw(gr);
		}

		protected override void DrawSelection(Graphics gr)
		{
			// No selection box!
		}

		// ******************

		public override void UpdatePath()
		{
			if (startPoint.X < endPoint.X)
			{
				lines[0].StartCap = StartCap;
				lines[0].EndCap = AvailableLineCap.None;
			}
			else
			{
				lines[0].StartCap = AvailableLineCap.None;
				lines[0].EndCap = StartCap;
			}

			if (startPoint.Y < endPoint.Y)
			{
				lines[1].StartCap = AvailableLineCap.None;
				lines[1].EndCap = EndCap;
			}
			else
			{
				lines[1].StartCap = EndCap;
				lines[1].EndCap = AvailableLineCap.None;
			}

			if (startPoint.X < endPoint.X)
			{
				lines[0].DisplayRectangle = new Rectangle(startPoint.X, startPoint.Y - BaseController.MIN_HEIGHT / 2, endPoint.X - startPoint.X, BaseController.MIN_HEIGHT);
			}
			else
			{
				lines[0].DisplayRectangle = new Rectangle(endPoint.X, startPoint.Y - BaseController.MIN_HEIGHT / 2, startPoint.X - endPoint.X, BaseController.MIN_HEIGHT);
			}

			if (startPoint.Y < endPoint.Y)
			{
				lines[1].DisplayRectangle = new Rectangle(endPoint.X - BaseController.MIN_WIDTH / 2, startPoint.Y, BaseController.MIN_WIDTH, endPoint.Y - startPoint.Y);
			}
			else
			{
				lines[1].DisplayRectangle = new Rectangle(endPoint.X - BaseController.MIN_WIDTH / 2, endPoint.Y, BaseController.MIN_WIDTH, startPoint.Y - endPoint.Y);
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
	}
}
