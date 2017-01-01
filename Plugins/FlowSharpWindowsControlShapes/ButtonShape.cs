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
    public class ButtonShape : ControlShape
    {
        public ButtonShape(Canvas canvas) : base(canvas)
        {
            control = new Button();
            canvas.Controls.Add(control);
            control.Click += OnClick;
        }

        private void OnClick(object sender, System.EventArgs e)
        {
            Send(ClickEventName);
        }

        public override void Draw(Graphics gr)
        {
            control.Visible = Visible;

            if (Visible)
            {
                base.Draw(gr);
                Rectangle r = DisplayRectangle.Grow(-4);
                control.Location = r.Location;
                control.Size = r.Size;
                control.Text = Text;
                control.Enabled = Enabled;
            }
        }
    }
}
