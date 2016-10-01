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
	public class TextShape : GraphicElement
	{
		public TextShape(Canvas canvas) : base(canvas)
		{
			Text = "[enter text]";
			HasCornerAnchors = false;
			HasCenterAnchors = false;
			HasTopBottomAnchors = false;
			HasLeftRightAnchors = false;
            BorderPen.Color = Color.White;
		}

		// Handle large font changes by calculating the new dimensions.
		public override void GetBackground()
		{
			UpdateDisplayRectangle(canvas.AntiAliasGraphics);
			base.GetBackground();
		}

		public override void Draw(Graphics gr)
		{
			UpdateDisplayRectangle(gr);
			gr.FillRectangle(FillBrush, DisplayRectangle);
			gr.DrawRectangle(BorderPen, DisplayRectangle);
			base.Draw(gr);
		}

		protected void UpdateDisplayRectangle(Graphics gr)
		{
			SizeF size = gr.MeasureString(Text, TextFont);
			Point center = DisplayRectangle.Center();
			// Grow so selection is not right on top of text, and so that anti-aliasing has some room.
			DisplayRectangle = new Rectangle(center.X - (int)(size.Width / 2), center.Y - (int)(size.Height) / 2, (int)size.Width, (int)size.Height).Grow(3);
		}
	}
}
