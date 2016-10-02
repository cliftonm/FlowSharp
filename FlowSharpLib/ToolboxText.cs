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
	/// <summary>
	/// Special rendering for this element in the toolbox only.
	/// </summary>
	public class ToolboxText : GraphicElement
	{
		public const string TOOLBOX_TEXT = "A";

		protected Brush brush = new SolidBrush(Color.Black);

		public ToolboxText(Canvas canvas) : base(canvas)
		{
			TextFont.Dispose();
			TextFont = new Font(FontFamily.GenericSansSerif, 20);
		}

		public override GraphicElement CloneDefault(Canvas canvas)
		{
			TextShape shape = new TextShape(canvas);

			return shape;
		}

		public override void Draw(Graphics gr)
		{
			SizeF size = gr.MeasureString(TOOLBOX_TEXT, TextFont);
			Point textpos = DisplayRectangle.Center().Move((int)(-size.Width / 2), (int)(-size.Height / 2));
			gr.DrawString(TOOLBOX_TEXT, TextFont, brush, textpos);
			base.Draw(gr);
		}
	}
}
