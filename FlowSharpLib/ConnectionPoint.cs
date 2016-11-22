/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Drawing;

namespace FlowSharpLib
{
	public enum GripType
	{
		None,
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
		LeftMiddle,
		RightMiddle,
		TopMiddle,
		BottomMiddle,

		// for lines:
		Start,
		End,
	};

	public class ConnectionPoint
	{
		// Setters should be protected, but serializer requires them to be public.
		public GripType Type { get; set; }
		public Point Point { get; set; }

		// Solely for serializer.
		public ConnectionPoint()
		{
		}

		public ConnectionPoint(GripType pos, Point p)
		{
			Type = pos;
			Point = p;
		}

        public static bool operator ==(ConnectionPoint cp1, ConnectionPoint cp2)
        {
            bool ret = false;

            // Alternate: object.ReferenceEquals(cp1, null), etc...
            if ((object)cp1 == null || (object)cp2 == null)
            {
                ret = (object)cp1 == (object)cp2;
            }
            else
            {
                return cp1.Type == cp2.Type && cp1.Point == cp2.Point;
            }

            return ret;
        }

        public static bool operator !=(ConnectionPoint cp1, ConnectionPoint cp2)
        {
            bool ret = false;

            // Alternate: object.ReferenceEquals(cp1, null), etc...
            if ((object)cp1 == null || (object)cp2 == null)
            {
                ret = (object)cp1 != (object)cp2;
            }
            else
            {
                ret = cp1.Type != cp2.Type || cp1.Point != cp2.Point;
            }

            return ret;
        }

        public override bool Equals(object obj)
        {
            bool ret = false;

            if (obj is ConnectionPoint)
            {
                ret = this == (ConnectionPoint)obj;
            }

            return ret;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
