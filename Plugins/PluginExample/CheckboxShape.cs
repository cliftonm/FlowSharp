/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Drawing;
using System.Windows.Forms;

using FlowSharpLib;

namespace PluginExample
{
    public class CheckboxShape : Box
    {
        protected CheckBox checkbox;

        public CheckboxShape(Canvas canvas) : base(canvas)
        {
            checkbox = new CheckBox();
            canvas.Controls.Add(checkbox);
        }

        public override void Draw(Graphics gr)
        {
            base.Draw(gr);
            Rectangle r = DisplayRectangle.Grow(-4);
            checkbox.Location = r.Location;
            checkbox.Size = r.Size;
            checkbox.Text = Text;
        }
    }
}
