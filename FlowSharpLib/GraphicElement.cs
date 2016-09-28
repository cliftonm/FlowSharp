using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FlowSharpLib
{
    public static class GraphicElementHelpers
    {
        public static void Erase(this Bitmap background, Canvas canvas, Rectangle r)
        {
            canvas.DrawImage(background, r);
            background.Dispose();
        }
	}

	/// <summary>
	/// Used for non-line shapes connecting to lines.
	/// </summary>
	public class Connection
	{
		public GraphicElement ToElement { get; set; }
		public ConnectionPoint ToConnectionPoint { get; set; }
		public ConnectionPoint ElementConnectionPoint { get; set; }
	}

	public class GraphicElement : IDisposable
    {
		public bool Selected { get; set; }
		public bool ShowConnectionPoints { get; set; }
		// public bool HideConnectionPoints { get; set; }
		public bool ShowAnchors { get; set; }

		// This is probably a ridiculous optimization -- should just grow pen width + connection point size / 2
		// public virtual Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(BorderPen.Width + ((ShowConnectionPoints || HideConnectionPoints) ? 3 : 0)); } }
		public virtual Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(BorderPen.Width + BaseController.CONNECTION_POINT_SIZE); } }
		public List<Connection> Connections = new List<Connection>();

		public Rectangle DisplayRectangle { get; set; }
		public Pen BorderPen { get; set; }
        public SolidBrush FillBrush { get; set; }

		protected bool HasCornerAnchors { get; set; }
		protected bool HasCenterAnchors { get; set; }
		protected bool HasLeftRightAnchors { get; set; }
		protected bool HasTopBottomAnchors { get; set; }

		protected bool HasCornerConnections { get; set; }
		protected bool HasCenterConnections { get; set; }
		protected bool HasLeftRightConnections { get; set; }
		protected bool HasTopBottomConnections { get; set; }

		protected Bitmap background;
        protected Rectangle backgroundRectangle;
        protected Pen selectionPen;
		protected Pen anchorPen = new Pen(Color.Black);
		protected Pen connectionPointPen = new Pen(Color.Blue);
		protected SolidBrush anchorBrush = new SolidBrush(Color.White);
		protected int anchorSize = 6;		// TODO: Make const?
		protected Canvas canvas;

		protected bool disposed;

        public GraphicElement(Canvas canvas)
        {
			this.canvas = canvas;
            selectionPen = new Pen(Color.Red);
			HasCenterAnchors = true;
			HasCornerAnchors = true;
			HasLeftRightAnchors = false;
			HasTopBottomAnchors = false;
			HasCenterConnections = true;
			HasCornerConnections = true;
			HasLeftRightConnections = false;
			HasTopBottomConnections = false;
			FillBrush = new SolidBrush(Color.White);
			BorderPen = new Pen(Color.Black);
			BorderPen.Width = 1;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				disposed = true;

				if (disposing)
				{
					BorderPen.Dispose();
					FillBrush.Dispose();
					background?.Dispose();
					selectionPen.Dispose();
					anchorPen.Dispose();
					anchorBrush.Dispose();
				}
			}
		}

		// TODO: Unify these into the second form at the call site.
		public virtual void MoveAnchor(ConnectionPoint cpShape, ConnectionPoint tocp) { }
		public virtual void MoveAnchor(GripType type, Point delta) { }

		public virtual ElementProperties CreateProperties()
		{
			return new ElementProperties(this);
		}

		public virtual Rectangle DefaultRectangle()
		{
			return new Rectangle(20, 20, 60, 60);
		}

		public virtual bool IsSelectable(Point p)
		{
			return UpdateRectangle.Contains(p);
		}

		/// <summary>
		/// Clone onto the specified canvas.
		/// </summary>
		public virtual GraphicElement Clone(Canvas canvas)
		{
			GraphicElement el = (GraphicElement)Activator.CreateInstance(GetType(), new object[] { canvas });
			el.DisplayRectangle = DisplayRectangle;
			el.UpdatePath();

			// Remove default because we're replacing with clone of element we're copying from.
			el.BorderPen.Dispose();
			el.FillBrush.Dispose();

			el.BorderPen = (Pen)BorderPen.Clone();
			el.FillBrush = (SolidBrush)FillBrush.Clone();

			return el;
		}

		public bool OnScreen(Rectangle r)
		{
			return canvas.OnScreen(r);
		}

		public bool OnScreen()
		{
			return canvas.OnScreen(UpdateRectangle);
		}

		public bool OnScreen(int dx, int dy)
		{
			return canvas.OnScreen(UpdateRectangle.Grow(dx, dy));
		}

		public virtual void UpdatePath() { }

        public virtual void Move(Point delta)
        {
            DisplayRectangle = DisplayRectangle.Move(delta);
        }

		public virtual void UpdateSize(ShapeAnchor anchor, Point delta)
		{
			canvas.Controller.UpdateSize(this, anchor, delta);
		}

        public virtual void GetBackground()
        {
            background?.Dispose();
			background = null;
			backgroundRectangle = canvas.Clip(UpdateRectangle);

			if (canvas.OnScreen(backgroundRectangle))
			{
				background = canvas.GetImage(backgroundRectangle);
			}
        }

        public virtual void CancelBackground()
        {
            background?.Dispose();
            background = null;
        }

		public virtual bool SnapCheck(ShapeAnchor anchor, Point delta)
		{
			UpdateSize(anchor, delta);
			canvas.Controller.UpdateSelectedElement.Fire(this, new ElementEventArgs() { Element = this });

			return false;
		}

		// Default returns true so we don't detach a shape's connectors when moving a shape.
		public virtual bool SnapCheck(GripType gt, ref Point delta) { return false; }

		// Placeholders:
		public virtual void MoveElementOrAnchor(GripType gt, Point delta) { }
		public virtual void SetConnection(GripType gt, GraphicElement shape) { }
		public virtual void RemoveConnection(GripType gt) { }
		public virtual void DisconnectShapeFromConnector(GripType gt) { }

		public virtual void Erase()
        {
            if (canvas.OnScreen(backgroundRectangle))
            {
                background?.Erase(canvas, backgroundRectangle);
                // canvas.Graphics.DrawRectangle(selectionPen, backgroundRectangle);
                background = null;
            }
        }

        public virtual void Draw()
        {
            if (canvas.OnScreen(UpdateRectangle))
            {
                Draw(canvas.AntiAliasGraphics);
            }

			if (ShowAnchors)
			{
				DrawAnchors();
			}

			if (ShowConnectionPoints)
			{
				DrawConnectionPoints();
			}
        }

		public virtual void UpdateScreen(int ix = 0, int iy = 0)
		{
			Rectangle r = canvas.Clip(UpdateRectangle.Grow(ix, iy));

			if (canvas.OnScreen(r))
			{
				canvas.CopyToScreen(r);
			}

			// We can now revert back to a smaller update rectangle if we are hiding connection points as a result
			// of an anchor moving out of range of the element.
			// HideConnectionPoints = false;
		}

		public virtual List<ShapeAnchor> GetAnchors()
		{
			List<ShapeAnchor> anchors = new List<ShapeAnchor>();
			Rectangle r;

			if (HasCornerAnchors)
			{
				r = new Rectangle(DisplayRectangle.TopLeftCorner(), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(GripType.TopLeft, r));
				r = new Rectangle(DisplayRectangle.TopRightCorner().Move(-anchorSize, 0), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(GripType.TopRight, r));
				r = new Rectangle(DisplayRectangle.BottomLeftCorner().Move(0, -anchorSize), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(GripType.BottomLeft, r));
				r = new Rectangle(DisplayRectangle.BottomRightCorner().Move(-anchorSize, -anchorSize), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(GripType.BottomRight, r));
			}

			if (HasCenterAnchors || HasLeftRightAnchors)
			{
				r = new Rectangle(DisplayRectangle.LeftMiddle().Move(0, -anchorSize / 2), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(GripType.LeftMiddle, r));
				r = new Rectangle(DisplayRectangle.RightMiddle().Move(-anchorSize, -anchorSize / 2), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(GripType.RightMiddle, r));
			}

			if (HasCenterAnchors || HasTopBottomAnchors)
			{ 
				r = new Rectangle(DisplayRectangle.TopMiddle().Move(-anchorSize / 2, 0), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(GripType.TopMiddle, r));
				r = new Rectangle(DisplayRectangle.BottomMiddle().Move(-anchorSize / 2, -anchorSize), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(GripType.BottomMiddle, r));
			}

			return anchors;
		}

		public virtual List<ConnectionPoint> GetConnectionPoints()
		{
			List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();

			if (HasCornerConnections)
			{
				connectionPoints.Add(new ConnectionPoint(GripType.TopLeft, DisplayRectangle.TopLeftCorner()));
				connectionPoints.Add(new ConnectionPoint(GripType.TopRight, DisplayRectangle.TopRightCorner()));
				connectionPoints.Add(new ConnectionPoint(GripType.BottomLeft, DisplayRectangle.BottomLeftCorner()));
				connectionPoints.Add(new ConnectionPoint(GripType.BottomRight, DisplayRectangle.BottomRightCorner()));
			}

			if (HasCenterConnections)
			{
				connectionPoints.Add(new ConnectionPoint(GripType.LeftMiddle, DisplayRectangle.LeftMiddle()));
				connectionPoints.Add(new ConnectionPoint(GripType.RightMiddle, DisplayRectangle.RightMiddle()));
				connectionPoints.Add(new ConnectionPoint(GripType.TopMiddle, DisplayRectangle.TopMiddle()));
				connectionPoints.Add(new ConnectionPoint(GripType.BottomMiddle, DisplayRectangle.BottomMiddle()));
			}

			if (HasLeftRightConnections)
			{
				connectionPoints.Add(new ConnectionPoint(GripType.Start, DisplayRectangle.LeftMiddle()));
				connectionPoints.Add(new ConnectionPoint(GripType.End, DisplayRectangle.RightMiddle()));
			}

			if (HasTopBottomConnections)
			{
				connectionPoints.Add(new ConnectionPoint(GripType.Start, DisplayRectangle.TopMiddle()));
				connectionPoints.Add(new ConnectionPoint(GripType.End, DisplayRectangle.BottomMiddle()));
			}

			return connectionPoints;
		}

		protected virtual void Draw(Graphics gr)
        {
            if (Selected)
            {
				// TODO: Visually, a box or a default dynamic connector's lines are obscured by the selection rectangle.
				Rectangle r = DisplayRectangle;
                gr.DrawRectangle(selectionPen, r);
            }
        }

		protected virtual void DrawAnchors()
		{
			GetAnchors().ForEach((a =>
			{
				canvas.Graphics.DrawRectangle(anchorPen, a.Rectangle);
				canvas.Graphics.FillRectangle(anchorBrush, a.Rectangle.Grow(-1));
			}));
		}

		protected virtual void DrawConnectionPoints()
		{
			GetConnectionPoints().ForEach(cp =>
			{
				canvas.AntiAliasGraphics.FillRectangle(anchorBrush, new Rectangle(cp.Point.X - BaseController.CONNECTION_POINT_SIZE, cp.Point.Y - BaseController.CONNECTION_POINT_SIZE, BaseController.CONNECTION_POINT_SIZE*2, BaseController.CONNECTION_POINT_SIZE*2));
				canvas.AntiAliasGraphics.DrawLine(connectionPointPen, cp.Point.X - BaseController.CONNECTION_POINT_SIZE, cp.Point.Y - BaseController.CONNECTION_POINT_SIZE, cp.Point.X + BaseController.CONNECTION_POINT_SIZE, cp.Point.Y + BaseController.CONNECTION_POINT_SIZE);
				canvas.AntiAliasGraphics.DrawLine(connectionPointPen, cp.Point.X + BaseController.CONNECTION_POINT_SIZE, cp.Point.Y - BaseController.CONNECTION_POINT_SIZE, cp.Point.X - BaseController.CONNECTION_POINT_SIZE, cp.Point.Y + BaseController.CONNECTION_POINT_SIZE);
			});
		}
	}
}
