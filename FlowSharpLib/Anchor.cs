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
	};

	public class Anchor
	{
		public const int PROXIMITY = 6;

		public AnchorPosition Type { get; protected set; }
		public Rectangle Rectangle { get; protected set; }

		public Anchor(AnchorPosition pos, System.Drawing.Rectangle r)
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
					ad = delta;
					break;
				case AnchorPosition.TopRight:
					ad = delta;
					break;
				case AnchorPosition.BottomLeft:
					ad = delta;
					break;
				case AnchorPosition.BottomRight:
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

		public Rectangle Resize(System.Drawing.Rectangle r, Point p)
		{
			int rx = r.X + r.Width;
			int ry = r.Y + r.Height;
			switch (Type)
			{
				case AnchorPosition.TopLeft:
					r = new System.Drawing.Rectangle(r.X + p.X, r.Y + p.Y, rx - r.X - p.X, ry - r.Y - p.Y);
					break;
				case AnchorPosition.TopRight:
					r = new System.Drawing.Rectangle(r.X, r.Y + p.Y, rx - r.X + p.X, ry - r.Y - p.Y);
					break;
				case AnchorPosition.BottomLeft:
					r = new System.Drawing.Rectangle(r.X + p.X, r.Y, rx - r.X - p.X, ry - r.Y + p.Y);
					break;
				case AnchorPosition.BottomRight:
					r = new System.Drawing.Rectangle(r.X, r.Y, rx - r.X + p.X, ry - r.Y + p.Y);
					break;

				case AnchorPosition.LeftMiddle:
					r = new System.Drawing.Rectangle(r.X + p.X, r.Y, rx - r.X - p.X, r.Height);
					break;
				case AnchorPosition.RightMiddle:
					r = new System.Drawing.Rectangle(r.X, r.Y, rx - r.X + p.X, r.Height);
					break;
				case AnchorPosition.TopMiddle:
					r = new System.Drawing.Rectangle(r.X, r.Y + p.Y, r.Width, ry - r.Y - p.Y);
					break;
				case AnchorPosition.BottomMiddle:
					r = new System.Drawing.Rectangle(r.X, r.Y, r.Width, ry - r.Y + p.Y);
					break;
			}

			return r;
		}
	}
}
