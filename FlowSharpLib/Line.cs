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
		public bool ShowLineAsSelected { get; set; }

        // See CustomLineCap for creating other possible endcaps besides arrows.
        // Note that AdjustableArrowCap derives from CustomLineCap!
        // https://msdn.microsoft.com/en-us/library/system.drawing.drawing2d.customlinecap(v=vs.110).aspx
        protected AdjustableArrowCap adjCapArrow;
        protected AdjustableArrowCap adjCapDiamond;

        public Line(Canvas canvas) : base(canvas)
		{
            adjCapArrow = new AdjustableArrowCap(BaseController.CAP_WIDTH, BaseController.CAP_HEIGHT, true);
            adjCapDiamond = new AdjustableArrowCap(BaseController.CAP_WIDTH, BaseController.CAP_HEIGHT, true);
            adjCapDiamond.MiddleInset = -BaseController.CAP_WIDTH;
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

        public override bool SnapCheck(ShapeAnchor anchor, Point delta)
		{
			bool ret = canvas.Controller.Snap(anchor.Type, ref delta);

			if (ret)
			{
                // Allow the entire line to move if snapped.
                Move(delta);
			}
			else
			{
                // Otherwise, move just the anchor point with axis constraints.
                ret = base.SnapCheck(anchor, delta);
			}

			return ret;
		}

		public override bool SnapCheck(GripType gt, ref Point delta)
		{
			return canvas.Controller.Snap(GripType.None, ref delta);
		}

		public override void MoveElementOrAnchor(GripType gt, Point delta)
		{
			canvas.Controller.MoveElement(this, delta);
		}
	}
}
