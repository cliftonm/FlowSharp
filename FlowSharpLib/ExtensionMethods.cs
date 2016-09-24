using System;
using System.Collections.Generic;
using System.Drawing;

namespace FlowSharpLib
{
    public static class ExtensionMethods
    {
        public static void Step(this int n, int step, Action<int> action)
        {
            for (int i = 0; i < n; i += step)
            {
                action(i);
            }
        }

        public static void Step2(this int n, int step, Action<int> action)
        {
            for (int i = 0; i < n + step; i += step)
            {
                action(i);
            }
        }

        public static Point Delta(this Point p, Point p2)
        {
            return new Point(p.X - p2.X, p.Y - p2.Y);
        }

        public static Point Add(this Point p, Point p2)
        {
            return new Point(p.X + p2.X, p.Y + p2.Y);
        }

        public static int Abs(this int n)
        {
            return Math.Abs(n);
        }

        public static int Min(this int a, int max)
        {
            return (a > max) ? max : a;
        }

        public static int Max(this int a, int min)
        {
            return (a < min) ? min : a;
        }

        public static Rectangle Grow(this System.Drawing.Rectangle r, float w)
        {
			System.Drawing.Rectangle ret = r;
            ret.Inflate((int)w, (int)w);

            return ret;
        }

        public static Rectangle Grow(this System.Drawing.Rectangle r, float x, float y)
        {
			System.Drawing.Rectangle ret = r;
            ret.Inflate((int)x, (int)y);

            return ret;
        }

		public static Point TopLeftCorner(this System.Drawing.Rectangle r)
		{
			return new Point(r.Left, r.Top);
		}

		public static Point TopRightCorner(this System.Drawing.Rectangle r)
		{
			return new Point(r.Right, r.Top);
		}

		public static Point BottomLeftCorner(this System.Drawing.Rectangle r)
		{
			return new Point(r.Left, r.Bottom);
		}

		public static Point BottomRightCorner(this System.Drawing.Rectangle r)
		{
			return new Point(r.Right, r.Bottom);
		}

		public static Point LeftMiddle(this System.Drawing.Rectangle r)
		{
			return new Point(r.Left, r.Top + r.Height / 2);
		}

		public static Point RightMiddle(this System.Drawing.Rectangle r)
		{
			return new Point(r.Right, r.Top + r.Height / 2);
		}

		public static Point TopMiddle(this System.Drawing.Rectangle r)
		{
			return new Point(r.Left + r.Width /2, r.Top);
		}

		public static Point BottomMiddle(this System.Drawing.Rectangle r)
		{
			return new Point(r.Left + r.Width / 2, r.Bottom);
		}

		public static Rectangle Move(this System.Drawing.Rectangle r, Point p)
		{
			r.Offset(p);

			return r;
		}

		public static Rectangle Move(this System.Drawing.Rectangle r, int x, int y)
		{
			r.Offset(x, y);

			return r;
		}

		public static Point Move(this Point r, Point p)
		{
			r.Offset(p);

			return r;
		}

		public static Point Move(this Point r, int x, int y)
		{
			r.Offset(x, y);

			return r;
		}

		public static int to_i(this float f)
        {
            return (int)f;
        }

        public static List<T> Swap<T>(this List<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;

            return list;
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }

		public static void Fire<TEventArgs>(this EventHandler<TEventArgs> theEvent, object sender, TEventArgs e = null) where TEventArgs : EventArgs
		{
			if (theEvent != null)
			{
				theEvent(sender, e);
			}
		}
	}
}
