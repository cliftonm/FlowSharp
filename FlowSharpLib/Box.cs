using System.Drawing;

namespace FlowSharpLib
{
    public class Box : GraphicElement
    {
        public Box(Canvas canvas) : base(canvas)
		{
        }

        public override void Draw(Graphics gr)
        {
            gr.FillRectangle(FillBrush, DisplayRectangle);
            gr.DrawRectangle(BorderPen, DisplayRectangle);
            base.Draw(gr);
        }
    }
}
