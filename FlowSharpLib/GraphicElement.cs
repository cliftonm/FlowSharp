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
		public bool HideConnectionPoints { get; set; }
		public bool ShowAnchors { get; set; }
        public virtual Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(BorderPen.Width); } }
		public List<Connection> Connections = new List<Connection>();

		public Rectangle DisplayRectangle { get; set; }
		public Pen BorderPen { get; set; }
        public SolidBrush FillBrush { get; set; }

		protected bool HasCornerAnchors { get; set; }
		protected bool HasCenterAnchors { get; set; }
		protected bool HasLeftRightAnchors { get; set; }
		protected bool HasTopBottomAnchors { get; set; }

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

		public virtual ElementProperties CreateProperties()
		{
			return new ElementProperties(this);
		}

		public virtual Rectangle DefaultRectangle()
		{
			return new Rectangle(20, 20, 60, 60);
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

			if (HideConnectionPoints)
			{
				UndrawConnectionPoints();
				HideConnectionPoints = false;
			}
        }

		public virtual void UpdateScreen(int ix = 0, int iy = 0)
		{
			Rectangle r = canvas.Clip(UpdateRectangle.Grow(ix, iy));

			if (canvas.OnScreen(r))
			{
				canvas.CopyToScreen(r);
			}
		}

		public virtual List<ShapeAnchor> GetAnchors()
		{
			List<ShapeAnchor> anchors = new List<ShapeAnchor>();
			Rectangle r;

			if (HasCornerAnchors)
			{
				r = new Rectangle(DisplayRectangle.TopLeftCorner(), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(AnchorPosition.TopLeft, r));
				r = new Rectangle(DisplayRectangle.TopRightCorner().Move(-anchorSize, 0), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(AnchorPosition.TopRight, r));
				r = new Rectangle(DisplayRectangle.BottomLeftCorner().Move(0, -anchorSize), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(AnchorPosition.BottomLeft, r));
				r = new Rectangle(DisplayRectangle.BottomRightCorner().Move(-anchorSize, -anchorSize), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(AnchorPosition.BottomRight, r));
			}

			if (HasCenterAnchors || HasLeftRightAnchors)
			{
				r = new Rectangle(DisplayRectangle.LeftMiddle().Move(0, -anchorSize / 2), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(AnchorPosition.LeftMiddle, r));
				r = new Rectangle(DisplayRectangle.RightMiddle().Move(-anchorSize, -anchorSize / 2), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(AnchorPosition.RightMiddle, r));
			}

			if (HasCenterAnchors || HasTopBottomAnchors)
			{ 
				r = new Rectangle(DisplayRectangle.TopMiddle().Move(-anchorSize / 2, 0), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(AnchorPosition.TopMiddle, r));
				r = new Rectangle(DisplayRectangle.BottomMiddle().Move(-anchorSize / 2, -anchorSize), new Size(anchorSize, anchorSize));
				anchors.Add(new ShapeAnchor(AnchorPosition.BottomMiddle, r));
			}

			return anchors;
		}

		public virtual List<ConnectionPoint> GetConnectionPoints()
		{
			List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();

			if (HasCornerAnchors)
			{
				connectionPoints.Add(new ConnectionPoint(ConnectionPosition.TopLeft, DisplayRectangle.TopLeftCorner()));
				connectionPoints.Add(new ConnectionPoint(ConnectionPosition.TopRight, DisplayRectangle.TopRightCorner()));
				connectionPoints.Add(new ConnectionPoint(ConnectionPosition.BottomLeft, DisplayRectangle.BottomLeftCorner()));
				connectionPoints.Add(new ConnectionPoint(ConnectionPosition.BottomRight, DisplayRectangle.BottomRightCorner()));
			}

			if (HasCenterAnchors)
			{
				connectionPoints.Add(new ConnectionPoint(ConnectionPosition.LeftMiddle, DisplayRectangle.LeftMiddle()));
				connectionPoints.Add(new ConnectionPoint(ConnectionPosition.RightMiddle, DisplayRectangle.RightMiddle()));
				connectionPoints.Add(new ConnectionPoint(ConnectionPosition.TopMiddle, DisplayRectangle.TopMiddle()));
				connectionPoints.Add(new ConnectionPoint(ConnectionPosition.BottomMiddle, DisplayRectangle.BottomMiddle()));
			}

			if (HasLeftRightAnchors)
			{
				connectionPoints.Add(new ConnectionPoint(ConnectionPosition.Start, DisplayRectangle.LeftMiddle()));
				connectionPoints.Add(new ConnectionPoint(ConnectionPosition.End, DisplayRectangle.RightMiddle()));
			}

			if (HasTopBottomAnchors)
			{
				connectionPoints.Add(new ConnectionPoint(ConnectionPosition.Start, DisplayRectangle.TopMiddle()));
				connectionPoints.Add(new ConnectionPoint(ConnectionPosition.End, DisplayRectangle.BottomMiddle()));
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
			// We specifically do NOT use the anti-aliasing graphics so that when we undraw the connection points, we don't leave residual smoothing pixels.
			// This problem will go away when the TODO in the UndrawConnectionPoints is resolved.
			GetConnectionPoints().ForEach(cp =>
			{
				canvas.Graphics.FillRectangle(anchorBrush, new Rectangle(cp.Point.X - BaseController.CONNECTION_POINT_SIZE, cp.Point.Y - BaseController.CONNECTION_POINT_SIZE, 10, 10));
				canvas.Graphics.DrawLine(connectionPointPen, cp.Point.X - BaseController.CONNECTION_POINT_SIZE, cp.Point.Y - BaseController.CONNECTION_POINT_SIZE, cp.Point.X + BaseController.CONNECTION_POINT_SIZE, cp.Point.Y + BaseController.CONNECTION_POINT_SIZE);
				canvas.Graphics.DrawLine(connectionPointPen, cp.Point.X + BaseController.CONNECTION_POINT_SIZE, cp.Point.Y - BaseController.CONNECTION_POINT_SIZE, cp.Point.X - BaseController.CONNECTION_POINT_SIZE, cp.Point.Y + BaseController.CONNECTION_POINT_SIZE);
			});
		}

		// TODO: This is a workaround dealing with the fact that connection points exceed the element boundaries, and we aren't
		// handling dealing with removing them with a background image that is larger than the element size.  So currently, this will
		// lead to possible "holes" and other artifacts at the pixel level.
		protected virtual void UndrawConnectionPoints()
		{
			GetConnectionPoints().ForEach(cp =>
			{
				Pen pen = new Pen(canvas.BackgroundColor);
				// diagonal down-right:
				canvas.Graphics.DrawLine(pen, cp.Point.X + 1, cp.Point.Y + 1, cp.Point.X + BaseController.CONNECTION_POINT_SIZE, cp.Point.Y + BaseController.CONNECTION_POINT_SIZE);
				// diagonal up-right:
				canvas.Graphics.DrawLine(pen, cp.Point.X + 1, cp.Point.Y - 1, cp.Point.X + BaseController.CONNECTION_POINT_SIZE, cp.Point.Y - BaseController.CONNECTION_POINT_SIZE);
				// diaganal up-left:
				canvas.Graphics.DrawLine(pen, cp.Point.X - 1, cp.Point.Y - 1, cp.Point.X - BaseController.CONNECTION_POINT_SIZE, cp.Point.Y - BaseController.CONNECTION_POINT_SIZE);
				// diagonal down-left:
				canvas.Graphics.DrawLine(pen, cp.Point.X - 1, cp.Point.Y + 1, cp.Point.X - BaseController.CONNECTION_POINT_SIZE, cp.Point.Y + BaseController.CONNECTION_POINT_SIZE);
				pen.Dispose();
			});
		}
	}
}
