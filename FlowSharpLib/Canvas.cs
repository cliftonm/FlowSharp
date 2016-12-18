/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Drawing;
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;

namespace FlowSharpLib
{
    public class Canvas : Panel
    {
		public Action<Canvas> PaintComplete { get; set; }
		public Color BackgroundColor { get { return canvasBrush.Color; } }
		public BaseController Controller { get; set; }
		public Bitmap Bitmap { get { return bitmap; } }

		protected SolidBrush canvasBrush;
        protected Pen gridPen;
        protected Size gridSpacing;
        protected Bitmap bitmap;
        protected Point origin = new Point(0, 0);
        protected Point dragOffset = new Point(0, 0);

		protected Graphics graphics;
		protected Graphics antiAliasGraphics;

        public Graphics Graphics { get { return graphics; } }
        public Graphics AntiAliasGraphics { get { return antiAliasGraphics; } }

        public Canvas()
        {
            DoubleBuffered = true;
            canvasBrush = new SolidBrush(Color.White);
            gridPen = new Pen(Color.LightBlue);
            gridSpacing = new Size(32, 32);
        }

        public void EndInit()
        {
            Paint += OnPaint;
        }

        protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				graphics.Dispose();
				antiAliasGraphics.Dispose();
				canvasBrush.Dispose();
				gridPen.Dispose();
				bitmap.Dispose();
			}
		}

        /// <summary>
        /// Canvas.Initialize requires that the parent be attached to the form!
        /// </summary>
        /// <param name="parent"></param>
		public void Initialize(Control parent)
        {
            Dock = DockStyle.Fill;
            parent.Controls.Add(this);

            if (NotMinimized())
            {
                CreateBitmap();
            }

            parent.Resize += (sndr, args) =>
            {
                if (NotMinimized())
                {
                    CreateBitmap();
                    Invalidate();
                }
            };
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
            Graphics grScreen = CreateGraphics();
            grScreen.DrawImage(b, r);
			b.Dispose();
            grScreen.Dispose();
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

		public void CreateBitmap(int w, int h)
		{
            bitmap?.Dispose();
            bitmap = new Bitmap(w, h);
			CreateGraphicsObjects();
		}

        protected bool NotMinimized()
        {
            return FindForm().WindowState != FormWindowState.Minimized && ClientSize.Width != 0 && ClientSize.Height != 0;
        }

		protected void CreateBitmap()
        {
            bitmap?.Dispose();
            bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
			CreateGraphicsObjects();
        }

		protected void CreateGraphicsObjects()
		{
            graphics?.Dispose();
			graphics = Graphics.FromImage(bitmap);
			antiAliasGraphics = Graphics.FromImage(bitmap);
			antiAliasGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
		}

		protected void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics gr = Graphics;
            DrawBackground(gr);
            DrawGrid(gr);
            PaintComplete(this);
            e.Graphics.DrawImage(bitmap, origin);
        }

        protected virtual void DrawBackground(Graphics gr)
        {
			gr.Clear(canvasBrush.Color);
        }

        protected virtual void DrawGrid(Graphics gr)
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
