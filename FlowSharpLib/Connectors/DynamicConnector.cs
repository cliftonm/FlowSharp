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
		protected Point StartPoint { get; set; }
		protected Point EndPoint { get; set; }

        public override void Select()
        {
            base.Select();
            lines.ForEach(l => l.ShowConnectorAsSelected = true);
        }

        public override void Deselect()
        {
            base.Deselect();
            lines.ForEach(l => l.ShowConnectorAsSelected = false);
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
            StartPoint = new Point(20, 20);
            EndPoint = new Point(60, 60);
            return base.DefaultRectangle();
        }

        public override bool IsSelectable(Point p)
        {
            return lines.Any(l => l.IsSelectable(p));
        }

        public override List<ConnectionPoint> GetConnectionPoints()
        {
            return new List<ConnectionPoint>() {
                new ConnectionPoint(GripType.Start, StartPoint),
                new ConnectionPoint(GripType.End, EndPoint),
            };
        }

        public override void Serialize(ElementPropertyBag epb, IEnumerable<GraphicElement> elementsBeingSerialized)
		{
			base.Serialize(epb, elementsBeingSerialized);
			epb.StartPoint = StartPoint;
			epb.EndPoint = EndPoint;
		}

		public override void Deserialize(ElementPropertyBag epb)
		{
			base.Deserialize(epb);
			StartPoint = epb.StartPoint;
			EndPoint = epb.EndPoint;
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
            bool ret = false;

            if (IsSnapToBeIgnored())
            {
                ret = base.SnapCheck(anchor, delta);
            }
            else
            {
                ret = canvas.Controller.Snap(anchor.Type, ref delta);

                if (ret)
                {
                    MoveAnchor(anchor.Type, delta);
                }
                else
                {
                    ret = base.SnapCheck(anchor, delta);
                }
            }

			return ret;
		}

		public override bool SnapCheck(GripType gt, ref Point delta)
		{
            if (IsSnapToBeIgnored()) return false;

            return canvas.Controller.Snap(GripType.None, ref delta);
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
            StartPoint = StartPoint.Move(delta);
            EndPoint = EndPoint.Move(delta);
            DisplayRectangle = RecalcDisplayRectangle();
        }

        // Executes when shape connected to this connector resizes.
        public override void MoveAnchor(ConnectionPoint cpShape, ConnectionPoint cp)
        {
            if (cp.Type == GripType.Start)
            {
                // X1
                // this.AnchorMoveUndoRedo(nameof(StartPoint), cpShape.Point, false);
                StartPoint = cpShape.Point;
            }
            else
            {
                // X1
                //this.AnchorMoveUndoRedo(nameof(EndPoint), cpShape.Point, false);
                EndPoint = cpShape.Point;
            }

            UpdatePath();
            DisplayRectangle = RecalcDisplayRectangle();
        }

        // Executes when shape connected to this connector moves.
        public override void MoveAnchor(GripType type, Point delta)
        {
            if (type == GripType.Start)
            {
                // X1
                //this.AnchorMoveUndoRedo(nameof(StartPoint), StartPoint.Move(delta), false);
                StartPoint = StartPoint.Move(delta);
            }
            else
            {
                // X1
                //this.AnchorMoveUndoRedo(nameof(EndPoint), EndPoint.Move(delta), false);
                EndPoint = EndPoint.Move(delta);
            }

            UpdatePath();
            DisplayRectangle = RecalcDisplayRectangle();
        }

        public override void UpdateSize(ShapeAnchor anchor, Point delta)
        {
            if (anchor.Type == GripType.Start)
            {
                // X1
                //this.AnchorMoveUndoRedo(nameof(StartPoint), StartPoint.Move(delta), false);
                StartPoint = StartPoint.Move(delta);
            }
            else
            {
                // X1
                //this.AnchorMoveUndoRedo(nameof(EndPoint), EndPoint.Move(delta), false);
                EndPoint = EndPoint.Move(delta);
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
            int x1 = StartPoint.X.Min(EndPoint.X);
            int y1 = StartPoint.Y.Min(EndPoint.Y);
            int x2 = StartPoint.X.Max(EndPoint.X);
            int y2 = StartPoint.Y.Max(EndPoint.Y);

            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }
        // ******************
    }
}
