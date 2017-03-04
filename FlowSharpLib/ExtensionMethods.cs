/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace FlowSharpLib
{
    public static class ExtensionMethods
    {
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

        public static Size Size(this Point p, Point p2)
        {
            return new Size(p.X - p2.X, p.Y - p2.Y);
        }

        public static Point Add(this Point p, Point p2)
        {
            return new Point(p.X + p2.X, p.Y + p2.Y);
        }

        public static int Abs(this int n)
        {
            return Math.Abs(n);
        }

        public static int Sign(this int n)
        {
            return Math.Sign(n);
        }

        //public static int Min(this int a, int max)
        //{
        //    return (a > max) ? max : a;
        //}

        //public static int Max(this int a, int min)
        //{
        //    return (a < min) ? min : a;
        //}
        
        public static Rectangle Grow(this Rectangle r, float w)
        {
            Rectangle ret = r;
            ret.Inflate((int)w, (int)w);

            return ret;
        }

        public static Point Center(this Rectangle r)
        {
            return new Point(r.X + r.Width / 2, r.Y + r.Height / 2);
        }

        public static bool IsNear(this Point p1, Point p2, int range)
        {
            return (p1.X - p2.X).Abs() <= range && (p1.Y - p2.Y).Abs() <= range;
        }

        public static Rectangle Grow(this Rectangle r, float x, float y)
        {
            Rectangle ret = r;
            ret.Inflate((int)x, (int)y);

            return ret;
        }

        public static Rectangle Union(this Rectangle r, Rectangle r2)
        {
            return Rectangle.Union(r, r2);
        }

        public static Point TopLeftCorner(this Rectangle r)
        {
            return new Point(r.Left, r.Top);
        }

        public static Point TopRightCorner(this Rectangle r)
        {
            return new Point(r.Right, r.Top);
        }

        public static Point BottomLeftCorner(this Rectangle r)
        {
            return new Point(r.Left, r.Bottom);
        }

        public static Point BottomRightCorner(this Rectangle r)
        {
            return new Point(r.Right, r.Bottom);
        }

        public static Point LeftMiddle(this Rectangle r)
        {
            return new Point(r.Left, r.Top + r.Height / 2);
        }

        public static Point RightMiddle(this Rectangle r)
        {
            return new Point(r.Right, r.Top + r.Height / 2);
        }

        public static Point TopMiddle(this Rectangle r)
        {
            return new Point(r.Left + r.Width / 2, r.Top);
        }

        public static Point BottomMiddle(this Rectangle r)
        {
            return new Point(r.Left + r.Width / 2, r.Bottom);
        }

        /// <summary>
        /// Return a new rectangle whose position is adjusted by p.
        /// </summary>
        public static Rectangle Add(this Rectangle r, Point p)
        {
            Rectangle ret = new Rectangle(r.X + p.X, r.Y + p.Y, r.Width, r.Height);

            return ret;
        }

        public static Rectangle Move(this Rectangle r, Point p)
        {
            r.Offset(p);

            return r;
        }

        public static Rectangle Move(this Rectangle r, int x, int y)
        {
            r.Offset(x, y);

            return r;
        }

        public static Point Move(this Point r, Point p)
        {
            r.Offset(p);

            return r;
        }

        public static Point Move(this Point r, Size sz)
        {
            r.Offset(sz.Width, sz.Height);

            return r;
        }

        public static Point Move(this Point r, int x, int y)
        {
            r.Offset(x, y);

            return r;
        }

        public static Point ReverseDirection(this Point p)
        {
            return new Point(-p.X, -p.Y);
        }

        public static int to_i(this float f)
        {
            return (int)f;
        }

        public static bool In<T>(this T item, T[] options)
        {
            return options.Contains(item);
        }

        public static void ForEachReverse<T>(this IList<T> collection, Action<T> action)
        {
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                action(collection[i]);
            }
        }

        public static List<T> Swap<T>(this List<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;

            return list;
        }
    }
}