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
