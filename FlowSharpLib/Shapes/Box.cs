/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Drawing;

namespace FlowSharpLib
{
    [ToolboxOrder(1)]
    public class Box : GraphicElement
    {
        public Box(Canvas canvas) : base(canvas)
		{
        }

        public override void Draw(Graphics gr, bool showSelection = true)
        {
            Rectangle zdr = ZoomRectangle;
            gr.FillRectangle(FillBrush, zdr);
            gr.DrawRectangle(BorderPen, zdr);
            base.Draw(gr, showSelection);
        }
    }
}
