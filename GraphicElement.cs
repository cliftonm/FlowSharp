using System.Collections.Generic;
using System.Drawing;

namespace FlowSharp
{
    public static class GraphicElementHelpers
    {
        public static void Erase(this Bitmap background, Canvas canvas, Rectangle r)
        {
            canvas.DrawImage(background, r);
            background.Dispose();
        }
	}

	public abstract class GraphicElement
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
        protected Rectangle backgroundRectangle;
        protected Pen selectionPen;
		protected Pen anchorPen = new Pen(Color.Black);
		protected SolidBrush anchorBrush = new SolidBrush(Color.White);
		protected int anchorSize = 6;
		protected Canvas canvas;

        public GraphicElement(Canvas canvas)
        {
			this.canvas = canvas;
            selectionPen = new Pen(Color.Red);
			HasCenterAnchors = true;
			HasCornerAnchors = true;
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

        public virtual void UpdateScreen(int ix = 0, int iy = 0)
        {
			Rectangle r = canvas.Clip(UpdateRectangle.Grow(ix, iy));

			if (canvas.OnScreen(r))
			{
				canvas.CopyToScreen(r);
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

			if (HasCenterAnchors)
			{
				r = new Rectangle(DisplayRectangle.LeftMiddle().Move(0, -anchorSize / 2), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.LeftMiddle, r));
				r = new Rectangle(DisplayRectangle.RightMiddle().Move(-anchorSize, -anchorSize / 2), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.RightMiddle, r));
				r = new Rectangle(DisplayRectangle.TopMiddle().Move(-anchorSize / 2, 0), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.TopMiddle, r));
				r = new Rectangle(DisplayRectangle.BottomMiddle().Move(-anchorSize / 2, -anchorSize), new Size(anchorSize, anchorSize));
				anchors.Add(new Anchor(AnchorPosition.BottomMiddle, r));
			}

			return anchors;
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
			GetAnchors().ForEach(a =>
			{
				canvas.Graphics.DrawRectangle(anchorPen, a.Rectangle);
				canvas.Graphics.FillRectangle(anchorBrush, a.Rectangle.Grow(-1));
			});
		}
    }
}
