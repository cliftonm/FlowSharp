using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace FlowSharp
{
    public class Canvas : Panel
    {
		public Action<Canvas> PaintComplete { get; set; }
		protected SolidBrush canvasBrush;
        protected Pen gridPen;
        protected Size gridSpacing;
        protected Bitmap bitmap;
        protected Point origin = new Point(0, 0);
        protected Point dragOffset = new Point(0, 0);

        public Graphics Graphics { get { return Graphics.FromImage(bitmap); } }
        public Graphics AntiAliasGraphics
        {
            get
            {
                Graphics gr = Graphics.FromImage(bitmap);
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                return gr;
            }
        }

        protected Canvas()
        {
            DoubleBuffered = true;
            canvasBrush = new SolidBrush(Color.White);
            gridPen = new Pen(Color.LightBlue);
            gridSpacing = new Size(32, 32);
            Paint += OnPaint;
        }

        public static Canvas Initialize(Control parent)
        {
            Canvas s = new Canvas();
            s.Dock = DockStyle.Fill;
            parent.Controls.Add(s);
            s.CreateBitmap();
            parent.Resize += (sndr, args) =>
            {
                s.bitmap.Dispose();
                s.CreateBitmap();
                s.Invalidate();
            };

            return s;
        }

        public void DrawImage(Bitmap img, Rectangle r)
        {
            Graphics.DrawImage(img, r);
        }

        public Bitmap GetImage(Rectangle r)
        {
            return bitmap.Clone(r, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        public void CopyToScreen(Rectangle r)
        {
			Bitmap b = bitmap.Clone(r, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			CreateGraphics().DrawImage(b, r);
			b.Dispose();
        }

        public bool OnScreen(Rectangle r)
        {
            return r.X < bitmap.Width && r.Y < bitmap.Height && r.Location.X + r.Width >= 0 && r.Location.Y + r.Height >= 0 && r.Width > 0 && r.Height > 0;
        }

        public void Drag(Point p)
        {
            dragOffset = new Point((dragOffset.X + p.X) % gridSpacing.Width, (dragOffset.Y + p.Y) % gridSpacing.Height);
        }

        public Rectangle Clip(Rectangle r)
        {
            int x = r.X.Max(0);
            int y = r.Y.Max(0);
            int width = (r.X + r.Width).Min(bitmap.Width) - r.X;
            int height = (r.Y + r.Height).Min(bitmap.Height) - r.Y;

            width += r.X - x;
            height += r.Y - y;

            return new Rectangle(x, y, width, height);
        }

        protected void CreateBitmap()
        {
            bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
        }

        protected void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics gr = Graphics;
            DrawBackground(gr);
            DrawGrid(gr);
            PaintComplete(this);
            e.Graphics.DrawImage(bitmap, origin);
        }

        protected void DrawBackground(Graphics gr)
        {
            gr.FillRectangle(canvasBrush, DisplayRectangle);
        }

        protected void DrawGrid(Graphics gr)
        {
            DrawVerticalGridLines(gr);
            DrawHorizontalGridLines(gr);
        }

        public void DrawVerticalGridLines(Graphics gr)
        { 
            DisplayRectangle.Height.Step2(gridSpacing.Height,
                ((y) =>
                    DisplayRectangle.Width.Step2(gridSpacing.Width, (x) =>
                        DrawLine(gr, gridPen, x+dragOffset.X, 0, x+dragOffset.X, DisplayRectangle.Height)
                )));
        }

        public void DrawHorizontalGridLines(Graphics gr)
        {
            DisplayRectangle.Width.Step2(gridSpacing.Width,
                ((x) =>
                    DisplayRectangle.Height.Step2(gridSpacing.Height, (y) =>
                        DrawLine(gr, gridPen, 0, y+dragOffset.Y, DisplayRectangle.Width, y+dragOffset.Y)
                )));
        }

        protected void DrawLine(Graphics gr, Pen pen, int x1, int y1, int x2, int y2)
        {
            gr.DrawLine(pen, x1, y1, x2, y2);
        }
    }
}
