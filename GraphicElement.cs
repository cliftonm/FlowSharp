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
        public Rectangle DisplayRectangle { get; set; }
        public Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(BorderPen.Width); } }
        public Pen BorderPen { get; set; }
        public SolidBrush FillBrush { get; set; }

        protected Bitmap background;
        protected Rectangle backgroundRectangle;
        protected Pen selectionPen;

        public GraphicElement()
        {
            selectionPen = new Pen(Color.Red);
        }

        public virtual void Move(Point p)
        {
            Rectangle r = DisplayRectangle;
            r.Offset(p);
            DisplayRectangle = r;
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
        }

        protected virtual void Draw(Graphics gr)
        {
            if (Selected)
            {
                Rectangle r = DisplayRectangle;
                gr.DrawRectangle(selectionPen, r);
            }
        }
    }
}
