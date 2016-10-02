/* The MIT License (MIT)
* 
* Copyright (c) 2016 Marc Clifton
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System.Drawing;

namespace FlowSharpLib
{
	public class ShapeAnchor
	{
		public const int PROXIMITY = 6;

		public GripType Type { get; protected set; }
		public Rectangle Rectangle { get; protected set; }

		public ShapeAnchor(GripType pos, Rectangle r)
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
