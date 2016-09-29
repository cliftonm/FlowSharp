using System;
using System.Drawing;
using System.Drawing.Drawing2D;

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

		public override GraphicElement Clone(Canvas canvas)
		{
			// TextShape dc = new TextShape(canvas, new Point(20, 20), new Point(60, 60));
			TextShape dc = new TextShape(canvas);

			return dc;
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
