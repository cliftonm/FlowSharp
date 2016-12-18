/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Drawing;
using System.Windows.Forms;

using FlowSharpLib;

namespace FlowSharpWindowsControlShapes
{
    public class CheckboxShape : ControlShape
    {
        public CheckboxShape(Canvas canvas) : base(canvas)
        {
            control = new CheckBox();
            canvas.Controls.Add(control);
        }

        public override void Draw(Graphics gr)
        {
            base.Draw(gr);
            Rectangle r = DisplayRectangle.Grow(-4);
            control.Location = r.Location;
            control.Size = r.Size;
            control.Text = Text;
        }
    }
}
