using System.Drawing;

namespace FlowSharpLib
{
    public class Box : GraphicElement
    {
        public Box(Canvas canvas) : base(canvas)
		{
            FillBrush = new SolidBrush(Color.White);
            BorderPen = new Pen(Color.Black);
            BorderPen.Width = 1;
        }

        protected override void Draw(Graphics gr)
        {
            gr.FillRectangle(FillBrush, DisplayRectangle);
            gr.DrawRectangle(BorderPen, DisplayRectangle);
            base.Draw(gr);
        }
    }
}
