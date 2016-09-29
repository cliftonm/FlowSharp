using System.Drawing;

namespace FlowSharpLib
{
    public class Ellipse : GraphicElement
    {
        public Ellipse(Canvas canvas) : base(canvas)
        {
			HasCornerConnections = false;
		}

		public override void Draw(Graphics gr)
        {
            gr.FillEllipse(FillBrush, DisplayRectangle);
            gr.DrawEllipse(BorderPen, DisplayRectangle);
            base.Draw(gr);
        }
    }
}
