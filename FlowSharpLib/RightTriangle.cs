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

using System.Collections.Generic;
using System.Drawing;

namespace FlowSharpLib
{
    public class RightTriangle : GraphicElement
    {
        protected Point[] path;

        public RightTriangle(Canvas canvas) : base(canvas)
        {
        }

        public override List<ConnectionPoint> GetConnectionPoints()
        {
            List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();
            connectionPoints.Add(new ConnectionPoint(GripType.Start, DisplayRectangle.LeftMiddle()));
            connectionPoints.Add(new ConnectionPoint(GripType.End, DisplayRectangle.RightMiddle()));
            connectionPoints.Add(new ConnectionPoint(GripType.Start, DisplayRectangle.TopLeftCorner()));
            connectionPoints.Add(new ConnectionPoint(GripType.End, DisplayRectangle.BottomLeftCorner()));

            return connectionPoints;
        }

        public override void UpdatePath()
        {
            path = new Point[]
            {
                new Point(DisplayRectangle.X + DisplayRectangle.Width,          DisplayRectangle.Y + DisplayRectangle.Height/2),        // right, middle
                new Point(DisplayRectangle.X,          DisplayRectangle.Y),                              // left, top
                new Point(DisplayRectangle.X,          DisplayRectangle.Y + DisplayRectangle.Height),          // left, bottom
                new Point(DisplayRectangle.X + DisplayRectangle.Width,                             DisplayRectangle.Y + DisplayRectangle.Height/2),        // right, middle
            };
        }

        public override void Draw(Graphics gr)
        {
            gr.FillPolygon(FillBrush, path);
            gr.DrawPolygon(BorderPen, path);
            base.Draw(gr);
        }
    }
}
