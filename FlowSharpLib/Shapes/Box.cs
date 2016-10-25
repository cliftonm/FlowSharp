/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

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
