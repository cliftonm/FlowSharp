using System.Collections.Generic;
using System.Drawing;

namespace FlowSharp
{
    public static class GraphicElementHelpers
    {
        public static void Erase(this Bitmap background, Surface surface, Rectangle r)
        {
            surface.DrawImage(background, r);
            background.Dispose();
        }
	}

	public abstract class GraphicElement
    {
        public bool Selected { get; set; }
		public bool ShowAnchors { get; set; }
        public Rectangle DisplayRectangle { get; set; }
        public Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(BorderPen.Width); } }
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

        public GraphicElement()
        {
            selectionPen = new Pen(Color.Red);
			HasCenterAnchors = true;
			HasCornerAnchors = true;
        }

		public virtual void UpdatePath()
		{
		}

        public virtual void Move(Point p)
        {
            DisplayRectangle = DisplayRectangle.Move(p);
        }

        public virtual void GetBackground(Surface surface)
        {
            background?.Dispose();
			background = null;
			backgroundRectangle = surface.Clip(UpdateRectangle);

			if (surface.OnScreen(backgroundRectangle))
			{
				background = surface.GetImage(backgroundRectangle);
			}
        }

        public virtual void CancelBackground()
        {
            background?.Dispose();
            background = null;
        }

        public virtual void Erase(Surface surface)
        {
            if (surface.OnScreen(backgroundRectangle))
            {
                background?.Erase(surface, backgroundRectangle);
                // surface.Graphics.DrawRectangle(selectionPen, backgroundRectangle);
                background = null;
            }
        }

        public virtual void UpdateScreen(Surface surface, int ix = 0, int iy = 0)
        {
			Rectangle r = surface.Clip(UpdateRectangle.Grow(ix, iy));

			if (surface.OnScreen(r))
			{
				surface.CopyToScreen(r);
			}
        }

        public virtual void Draw(Surface surface)
        {
            if (surface.OnScreen(UpdateRectangle))
            {
                Draw(surface.AntiAliasGraphics);
            }

			if (ShowAnchors)
			{
				DrawAnchors(surface);
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

		protected virtual void DrawAnchors(Surface surface)
		{
			GetAnchors().ForEach(a =>
			{
				surface.Graphics.DrawRectangle(anchorPen, a.Rectangle);
				surface.Graphics.FillRectangle(anchorBrush, a.Rectangle.Grow(-1));
			});
		}
    }
}
