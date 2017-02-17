/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Drawing;
using System.Windows.Forms;

namespace FlowSharpLib
{
    [ExcludeFromToolbox]
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

        public override void Draw(Graphics gr, bool showSelection = true)
		{
			UpdateDisplayRectangle(gr);
			gr.FillRectangle(FillBrush, DisplayRectangle);
			gr.DrawRectangle(BorderPen, DisplayRectangle);
			base.Draw(gr, showSelection);
		}

		protected void UpdateDisplayRectangle(Graphics gr)
		{
            SizeF size = TextRenderer.MeasureText(gr, Text, TextFont);
            // SizeF size = gr.MeasureString(Text, TextFont);
            // Grow so selection is not right on top of text, and so that anti-aliasing has some room.
            // Point center = DisplayRectangle.Center();
            // DisplayRectangle = new Rectangle(center.X - (int)(size.Width / 2), center.Y - (int)(size.Height) / 2, (int)size.Width, (int)size.Height).Grow(3);
            DisplayRectangle = new Rectangle(DisplayRectangle.X+3, DisplayRectangle.Y+3, (int)size.Width, (int)size.Height).Grow(3);
        }
	}

    /// <summary>
    /// Special rendering for this element in the toolbox only.
    /// </summary>
    [ToolboxShape]
    [ToolboxOrder(13)]
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
            return CloneDefault(canvas, Point.Empty);
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
            TextShape shape = new TextShape(canvas);
            shape.DisplayRectangle = shape.DefaultRectangle().Move(offset);
            shape.UpdateProperties();
            shape.UpdatePath();

            return shape;
        }

        public override void Draw(Graphics gr, bool showSelection = true)
        {
            // Use ContentAlignment to position text.
            SizeF size = gr.MeasureString(TOOLBOX_TEXT, TextFont);
            Point textpos = DisplayRectangle.Center().Move((int)(-size.Width / 2), (int)(-size.Height / 2));
            gr.DrawString(TOOLBOX_TEXT, TextFont, brush, textpos);
            base.Draw(gr, showSelection);
        }
    }
}
