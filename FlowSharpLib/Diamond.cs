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
	public class Diamond : GraphicElement
	{
		protected Point[] path;

		public Diamond(Canvas canvas) : base(canvas)
		{
			HasCornerConnections = false;
		}

		public override void UpdatePath()
		{
			path = new Point[]
			{
				new Point(DisplayRectangle.X,                             DisplayRectangle.Y + DisplayRectangle.Height/2),
				new Point(DisplayRectangle.X + DisplayRectangle.Width/2,		DisplayRectangle.Y),
				new Point(DisplayRectangle.X + DisplayRectangle.Width,    DisplayRectangle.Y + DisplayRectangle.Height/2),
				new Point(DisplayRectangle.X + DisplayRectangle.Width/2,		DisplayRectangle.Y + DisplayRectangle.Height),
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
