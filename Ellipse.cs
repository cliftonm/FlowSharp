using System.Drawing;
using System.Windows.Forms;

namespace FlowSharp
{
    public class Ellipse : GraphicElement
    {
        public Ellipse(Canvas canvas) : base(canvas)
        {
            FillBrush = new SolidBrush(Color.Blue);
            BorderPen = new Pen(Color.Black);
        }


        protected override void Draw(Graphics gr)
        {
            gr.FillEllipse(FillBrush, DisplayRectangle);
            gr.DrawEllipse(BorderPen, DisplayRectangle);
            base.Draw(gr);
        }
    }
}
