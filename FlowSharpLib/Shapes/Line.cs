/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Drawing;
using System.Drawing.Drawing2D;

namespace FlowSharpLib
{
	public abstract class Line : Connector
	{
        public Line(Canvas canvas) : base(canvas)
		{
        }

		public override ElementProperties CreateProperties()
		{
			return new LineProperties(this);
		}

        public override void UpdateProperties()
        {
            if (StartCap == AvailableLineCap.None)
            {
                BorderPen.StartCap = LineCap.NoAnchor;
            }
            else
            {
                BorderPen.CustomStartCap = StartCap == AvailableLineCap.Arrow ? adjCapArrow : adjCapDiamond;
            }

            if (EndCap == AvailableLineCap.None)
            {
                BorderPen.EndCap = LineCap.NoAnchor;
            }
            else
            {
                BorderPen.CustomEndCap = EndCap == AvailableLineCap.Arrow ? adjCapArrow : adjCapDiamond;
            }

            base.UpdateProperties();
        }
    }
}
