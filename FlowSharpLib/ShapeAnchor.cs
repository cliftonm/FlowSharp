/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Drawing;
using System.Windows.Forms;

namespace FlowSharpLib
{
	public class ShapeAnchor
	{
		public const int PROXIMITY = 6;

		public GripType Type { get; protected set; }
		public Rectangle Rectangle { get; protected set; }
        public Cursor Cursor { get; protected set; }

		public ShapeAnchor(GripType pos, Rectangle r, Cursor cursor)
		{
			Type = pos;
			Rectangle = r;
            Cursor = cursor;
		}

		public bool Near(Point p)
		{
			int cx = Rectangle.X + Rectangle.Width / 2;
			int cy = Rectangle.Y + Rectangle.Height / 2;

			return (p.X - cx).Abs() <= PROXIMITY && (p.Y - cy).Abs() <= PROXIMITY;
		}

		public Point AdjustedDelta(Point delta)
		{
			Point ad = Point.Empty;

			switch (Type)
			{
				case GripType.TopLeft:
				case GripType.TopRight:
				case GripType.BottomLeft:
				case GripType.BottomRight:
				case GripType.Start:
				case GripType.End:
					ad = delta;
					break;

				case GripType.LeftMiddle:
					ad = new Point(delta.X, 0);
					break;
				case GripType.RightMiddle:
					ad = new Point(delta.X, 0);
					break;
				case GripType.TopMiddle:
					ad = new Point(0, delta.Y);
					break;
				case GripType.BottomMiddle:
					ad = new Point(0, delta.Y);
					break;
			}

			return ad;
		}

		public Rectangle Resize(Rectangle r, Point p)
		{
			int rx = r.X + r.Width;
			int ry = r.Y + r.Height;

			switch (Type)
			{
				case GripType.TopLeft:
					{
                        // This here and in other cases prevents "shoving" when mins are reached.
                        int w = (rx - r.X - p.X).Max(BaseController.MIN_WIDTH);
						int h = (ry - r.Y - p.Y).Max(BaseController.MIN_HEIGHT);
						if (w == BaseController.MIN_WIDTH) p.X = 0;
						if (h == BaseController.MIN_HEIGHT) p.Y = 0;
						r = new Rectangle(r.X + p.X, r.Y + p.Y, w, h);
						break;
					}
				case GripType.TopRight:
					{
						int h = (ry - r.Y - p.Y).Max(BaseController.MIN_HEIGHT);
						if (h == BaseController.MIN_HEIGHT) p.Y = 0;
						r = new Rectangle(r.X, r.Y + p.Y, (rx - r.X + p.X).Max(BaseController.MIN_WIDTH), h);
						break;
					}
				case GripType.BottomLeft:
					{
						int w = (rx - r.X - p.X).Max(BaseController.MIN_WIDTH);
						if (w == BaseController.MIN_WIDTH) p.X = 0;
						r = new Rectangle(r.X + p.X, r.Y, w, (ry - r.Y + p.Y).Max(BaseController.MIN_HEIGHT));
						break;
					}
				case GripType.BottomRight:
					r = new Rectangle(r.X, r.Y, (rx - r.X + p.X).Max(BaseController.MIN_WIDTH), (ry - r.Y + p.Y).Max(BaseController.MIN_HEIGHT));
					break;

				case GripType.LeftMiddle:
					{
						int w = (rx - r.X - p.X).Max(BaseController.MIN_WIDTH);
						if (w == BaseController.MIN_WIDTH) p.X = 0;
						r = new Rectangle(r.X + p.X, r.Y, w, r.Height);
						break;
					}
				case GripType.RightMiddle:
					r = new Rectangle(r.X, r.Y, (rx - r.X + p.X).Max(BaseController.MIN_WIDTH), r.Height);
					break;
				case GripType.TopMiddle:
					{
						int h = (ry - r.Y - p.Y).Max(BaseController.MIN_HEIGHT);
						if (h == BaseController.MIN_HEIGHT) p.Y = 0;
						r = new Rectangle(r.X, r.Y + p.Y, r.Width, h);
						break;
					}
				case GripType.BottomMiddle:
					r = new Rectangle(r.X, r.Y, r.Width, (ry - r.Y + p.Y).Max(BaseController.MIN_HEIGHT));
					break;

				case GripType.Start:
					r = new Rectangle(r.X + p.X, r.Y + p.Y, r.Width - p.X, r.Height - p.Y);
					break;
				case GripType.End:
					r = new Rectangle(r.X, r.Y, r.Width + p.X, r.Height + p.Y);
					break;
			}

			return r;
		}
	}
}
