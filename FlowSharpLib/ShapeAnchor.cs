using System.Drawing;

namespace FlowSharpLib
{
	public enum AnchorPosition
	{
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
		LeftMiddle,
		RightMiddle,
		TopMiddle,
		BottomMiddle,

		// For dynamic connector:
		Start,
		End,
	};

	public class ShapeAnchor
	{
		public const int PROXIMITY = 6;

		public AnchorPosition Type { get; protected set; }
		public Rectangle Rectangle { get; protected set; }

		public ShapeAnchor(AnchorPosition pos, Rectangle r)
		{
			Type = pos;
			Rectangle = r;
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
				case AnchorPosition.TopLeft:
				case AnchorPosition.TopRight:
				case AnchorPosition.BottomLeft:
				case AnchorPosition.BottomRight:
				case AnchorPosition.Start:
				case AnchorPosition.End:
					ad = delta;
					break;

				case AnchorPosition.LeftMiddle:
					ad = new Point(delta.X, 0);
					break;
				case AnchorPosition.RightMiddle:
					ad = new Point(delta.X, 0);
					break;
				case AnchorPosition.TopMiddle:
					ad = new Point(0, delta.Y);
					break;
				case AnchorPosition.BottomMiddle:
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
				case AnchorPosition.TopLeft:
					{
						// This here and in other cases prevents "shoving" when mins are reached.
						int w = (rx - r.X - p.X).Max(BaseController.MIN_WIDTH);
						int h = (ry - r.Y - p.Y).Max(BaseController.MIN_HEIGHT);
						if (w == BaseController.MIN_WIDTH) p.X = 0;
						if (h == BaseController.MIN_HEIGHT) p.Y = 0;
						r = new Rectangle(r.X + p.X, r.Y + p.Y, w, h);
						break;
					}
				case AnchorPosition.TopRight:
					{
						int h = (ry - r.Y - p.Y).Max(BaseController.MIN_HEIGHT);
						if (h == BaseController.MIN_HEIGHT) p.Y = 0;
						r = new Rectangle(r.X, r.Y + p.Y, (rx - r.X + p.X).Max(BaseController.MIN_WIDTH), h);
						break;
					}
				case AnchorPosition.BottomLeft:
					{
						int w = (rx - r.X - p.X).Max(BaseController.MIN_WIDTH);
						if (w == BaseController.MIN_WIDTH) p.X = 0;
						r = new Rectangle(r.X + p.X, r.Y, w, (ry - r.Y + p.Y).Max(BaseController.MIN_HEIGHT));
						break;
					}
				case AnchorPosition.BottomRight:
					r = new Rectangle(r.X, r.Y, (rx - r.X + p.X).Max(BaseController.MIN_WIDTH), (ry - r.Y + p.Y).Max(BaseController.MIN_HEIGHT));
					break;

				case AnchorPosition.LeftMiddle:
					{
						int w = (rx - r.X - p.X).Max(BaseController.MIN_WIDTH);
						if (w == BaseController.MIN_WIDTH) p.X = 0;
						r = new Rectangle(r.X + p.X, r.Y, w, r.Height);
						break;
					}
				case AnchorPosition.RightMiddle:
					r = new Rectangle(r.X, r.Y, (rx - r.X + p.X).Max(BaseController.MIN_WIDTH), r.Height);
					break;
				case AnchorPosition.TopMiddle:
					{
						int h = (ry - r.Y - p.Y).Max(BaseController.MIN_HEIGHT);
						if (h == BaseController.MIN_HEIGHT) p.Y = 0;
						r = new Rectangle(r.X, r.Y + p.Y, r.Width, h);
						break;
					}
				case AnchorPosition.BottomMiddle:
					r = new Rectangle(r.X, r.Y, r.Width, (ry - r.Y + p.Y).Max(BaseController.MIN_HEIGHT));
					break;

				case AnchorPosition.Start:
					r = new Rectangle(r.X + p.X, r.Y + p.Y, r.Width - p.X, r.Height - p.Y);
					break;
				case AnchorPosition.End:
					r = new Rectangle(r.X, r.Y, r.Width + p.X, r.Height + p.Y);
					break;
			}

			return r;
		}
	}
}
