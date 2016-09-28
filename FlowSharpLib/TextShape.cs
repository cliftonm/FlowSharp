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
			// PropertiesChanged += (sndr, args) => Erase();
		}

		protected override void Draw(Graphics gr)
		{
			UpdateDisplayRectangle(gr);
			GetBackground();		// Update the background before we draw so font size changes capture the new background first.
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
