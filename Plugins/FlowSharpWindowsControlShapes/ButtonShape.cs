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
    public class ButtonShape : Box
    {
        protected Button button;

        public ButtonShape(Canvas canvas) : base(canvas)
        {
            button = new Button();
            canvas.Controls.Add(button);
            button.Click += OnClick;
        }

        private void OnClick(object sender, System.EventArgs e)
        {
            string url = "http://localhost:8002/ButtonClick?ShapeName=" + Name;
            Http.Get(url);
        }

        public override void Draw(Graphics gr)
        {
            base.Draw(gr);
            Rectangle r = DisplayRectangle.Grow(-4);
            button.Location = r.Location;
            button.Size = r.Size;
            button.Text = Text;
        }
    }
}
