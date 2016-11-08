/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FlowSharpLib
{
	public abstract class DynamicConnector : Connector
	{
		protected List<Line> lines = new List<Line>();
		protected Point startPoint;
		protected Point endPoint;

		public override bool Selected
		{
			get { return base.Selected; }
			set
			{
				base.Selected = value;
				lines.ForEach(l => l.ShowConnectorAsSelected = value);
			}
		}

		public DynamicConnector(Canvas canvas) : base(canvas)
		{
			HasCornerAnchors = false;
			HasCenterAnchors = false;
			HasTopBottomAnchors = false;
			HasLeftRightAnchors = false;
		}

		protected override void Dispose(bool disposing)
		{
            if (!disposed && disposing)
			{
				lines.ForEach(l => l.Dispose());
			}

			base.Dispose(disposing);
		}

        public override Rectangle DefaultRectangle()
        {
            startPoint = new Point(20, 20);
            endPoint = new Point(60, 60);
            return base.DefaultRectangle();
        }

        public override bool IsSelectable(Point p)
        {
            return lines.Any(l => l.IsSelectable(p));
        }

        public override List<ConnectionPoint> GetConnectionPoints()
        {
            return new List<ConnectionPoint>() {
                new ConnectionPoint(GripType.Start, startPoint),
                new ConnectionPoint(GripType.End, endPoint),
            };
        }

        public override void Serialize(ElementPropertyBag epb, IEnumerable<GraphicElement> elementsBeingSerialized)
		{
			base.Serialize(epb, elementsBeingSerialized);
			epb.StartPoint = startPoint;
			epb.EndPoint = endPoint;
		}

		public override void Deserialize(ElementPropertyBag epb)
		{
			base.Deserialize(epb);
			startPoint = epb.StartPoint;
			endPoint = epb.EndPoint;
		}

		public override ElementProperties CreateProperties()
		{
			return new DynamicConnectorProperties(this);
		}

		public override void UpdateProperties()
		{
			lines.ForEach(l =>
			{
                l.BorderPen.Color = BorderPen.Color;
                l.BorderPen.Width = BorderPen.Width;
                // was:
                // l.Dispose();
				// l.BorderPen = new Pen(BorderPen.Color, BorderPen.Width);
			});
		}

		public override bool SnapCheck(ShapeAnchor anchor, Point delta)
		{
            if (IsSnapToBeIgnored()) return false;

			bool ret = canvas.Controller.Snap(anchor.Type, ref delta);

			if (ret)
			{
				MoveAnchor(anchor.Type, delta);
			}
			else
			{
				ret = base.SnapCheck(anchor, delta);
			}

			return ret;
		}


		public override bool SnapCheck(GripType gt, ref Point delta)
		{
            if (IsSnapToBeIgnored()) return false;

            return canvas.Controller.Snap(GripType.None, ref delta);
		}

		public override void MoveElementOrAnchor(GripType gt, Point delta)
		{
			MoveAnchor(gt, delta);
		}

		public override void SetCanvas(Canvas canvas)
		{
			lines.ForEach(l => l.SetCanvas(canvas));
			base.SetCanvas(canvas);
		}

		// Dynamic connector does not update it's region, only the lines composing the connector do.
		protected override void DrawUpdateRectangle(Graphics gr) { }

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
                startPoint = cpShape.Point;
            }
            else
            {
                endPoint = cpShape.Point;
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

        public override void Draw(Graphics gr)
        {
            lines.ForEach(l => l.Draw());

            // No selection box!
            // base.Draw(gr);
        }


        protected Rectangle RecalcDisplayRectangle()
        {
            int x1 = startPoint.X.Min(endPoint.X);
            int y1 = startPoint.Y.Min(endPoint.Y);
            int x2 = startPoint.X.Max(endPoint.X);
            int y2 = startPoint.Y.Max(endPoint.Y);

            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }
        // ******************
    }
}
