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

	public class GraphicElement : IDisposable
    {
		public bool Selected { get; set; }
		public bool ShowAnchors { get; set; }
        public Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(BorderPen.Width); } }

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
		protected SolidBrush anchorBrush = new SolidBrush(Color.White);
		protected int anchorSize = 6;
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
			return DisplayRectangle;
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
        }

		public virtual void UpdateScreen(int ix = 0, int iy = 0)
		{
			Rectangle r = canvas.Clip(UpdateRectangle.Grow((float)ix, (float)iy));

			if (canvas.OnScreen(r))
			{
				canvas.CopyToScreen(r);
			}
		}

		public virtual List<Anchor> GetAnchors()
		{
			List<Anchor> anchors = new List<Anchor>();
			Rectangle r;

			if (HasCornerAnchors)
			{
				r = new Rectangle(DisplayRectangle.TopLeftCorner(), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.TopLeft, r));
				r = new Rectangle(DisplayRectangle.TopRightCorner().Move(-anchorSize, 0), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.TopRight, r));
				r = new Rectangle(DisplayRectangle.BottomLeftCorner().Move(0, -anchorSize), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.BottomLeft, r));
				r = new Rectangle(DisplayRectangle.BottomRightCorner().Move(-anchorSize, -anchorSize), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.BottomRight, r));
			}

			if (HasCenterAnchors || HasLeftRightAnchors)
			{
				r = new Rectangle(DisplayRectangle.LeftMiddle().Move(0, -anchorSize / 2), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.LeftMiddle, r));
				r = new Rectangle(DisplayRectangle.RightMiddle().Move(-anchorSize, -anchorSize / 2), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.RightMiddle, r));
			}

			if (HasCenterAnchors || HasTopBottomAnchors)
			{ 
				r = new Rectangle(DisplayRectangle.TopMiddle().Move(-anchorSize / 2, 0), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.TopMiddle, r));
				r = new Rectangle(DisplayRectangle.BottomMiddle().Move(-anchorSize / 2, -anchorSize), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.BottomMiddle, r));
			}

			return anchors;
		}

		public virtual List<Point> GetConnectionPoints()
		{
			List<Point> connectionPoints = new List<Point>();
			Rectangle r;

			if (HasCornerAnchors)
			{
				connectionPoints.Add(DisplayRectangle.TopLeftCorner());
				connectionPoints.Add(DisplayRectangle.TopRightCorner());
				connectionPoints.Add(DisplayRectangle.BottomLeftCorner());
				connectionPoints.Add(DisplayRectangle.BottomRightCorner());
			}

			if (HasCenterAnchors || HasLeftRightAnchors)
			{
				connectionPoints.Add(DisplayRectangle.LeftMiddle());
				connectionPoints.Add(DisplayRectangle.RightMiddle());
			}

			if (HasCenterAnchors || HasTopBottomAnchors)
			{
				connectionPoints.Add(DisplayRectangle.TopMiddle());
				connectionPoints.Add(DisplayRectangle.BottomMiddle());
			}

			return connectionPoints;
		}

		protected virtual void Draw(Graphics gr)
        {
            if (Selected)
            {
				Rectangle r = DisplayRectangle;
                gr.DrawRectangle(selectionPen, r);
            }
        }

		protected virtual void DrawAnchors()
		{
			GetAnchors().ForEach((Action<Anchor>)(a =>
			{
				canvas.Graphics.DrawRectangle(anchorPen, (Rectangle)a.Rectangle);
				canvas.Graphics.FillRectangle(anchorBrush, (Rectangle)ExtensionMethods.Grow(a.Rectangle, (float)-1));
			}));
		}
    }
}
