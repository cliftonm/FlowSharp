using System;
using System.Collections.Generic;
using System.Drawing;

namespace FlowSharpLib
{
    public static class GraphicElementHelpers
    {
        public static void Erase(this Bitmap background, Canvas canvas, System.Drawing.Rectangle r)
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

        protected Bitmap background;
        protected System.Drawing.Rectangle backgroundRectangle;
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

		/// <summary>
		/// Clone onto the specified canvas.
		/// </summary>
		public GraphicElement Clone(Canvas canvas)
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

		public bool OnScreen(System.Drawing.Rectangle r)
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

		public virtual void UpdatePath()
		{
		}

        public virtual void Move(Point p)
        {
            DisplayRectangle = DisplayRectangle.Move(p);
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
			System.Drawing.Rectangle r = canvas.Clip((System.Drawing.Rectangle)ExtensionMethods.Grow(UpdateRectangle, (float)ix, (float)iy));

			if (canvas.OnScreen(r))
			{
				canvas.CopyToScreen(r);
			}
		}

		public virtual List<Anchor> GetAnchors()
		{
			List<Anchor> anchors = new List<Anchor>();
			System.Drawing.Rectangle r;

			if (HasCornerAnchors)
			{
				r = new System.Drawing.Rectangle(ExtensionMethods.TopLeftCorner(DisplayRectangle), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.TopLeft, r));
				r = new System.Drawing.Rectangle(ExtensionMethods.TopRightCorner(DisplayRectangle).Move(-anchorSize, 0), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.TopRight, r));
				r = new System.Drawing.Rectangle(ExtensionMethods.BottomLeftCorner(DisplayRectangle).Move(0, -anchorSize), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.BottomLeft, r));
				r = new System.Drawing.Rectangle(ExtensionMethods.BottomRightCorner(DisplayRectangle).Move(-anchorSize, -anchorSize), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.BottomRight, r));
			}

			if (HasCenterAnchors)
			{
				r = new System.Drawing.Rectangle(ExtensionMethods.LeftMiddle(DisplayRectangle).Move(0, -anchorSize / 2), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.LeftMiddle, r));
				r = new System.Drawing.Rectangle(ExtensionMethods.RightMiddle(DisplayRectangle).Move(-anchorSize, -anchorSize / 2), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.RightMiddle, r));
				r = new System.Drawing.Rectangle(ExtensionMethods.TopMiddle(DisplayRectangle).Move(-anchorSize / 2, 0), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.TopMiddle, r));
				r = new System.Drawing.Rectangle(ExtensionMethods.BottomMiddle(DisplayRectangle).Move(-anchorSize / 2, -anchorSize), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.BottomMiddle, r));
			}

			return anchors;
		}


		protected virtual void Draw(Graphics gr)
        {
            if (Selected)
            {
				System.Drawing.Rectangle r = DisplayRectangle;
                gr.DrawRectangle(selectionPen, r);
            }
        }

		protected virtual void DrawAnchors()
		{
			GetAnchors().ForEach((Action<Anchor>)(a =>
			{
				canvas.Graphics.DrawRectangle(anchorPen, (System.Drawing.Rectangle)a.Rectangle);
				canvas.Graphics.FillRectangle(anchorBrush, (System.Drawing.Rectangle)ExtensionMethods.Grow(a.Rectangle, (float)-1));
			}));
		}
    }
}
