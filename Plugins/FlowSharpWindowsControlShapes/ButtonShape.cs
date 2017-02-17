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
    [ExcludeFromToolbox]
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

        public override void Draw(Graphics gr, bool showSelection = true)
        {
            control.Visible = Visible;

            if (Visible)
            {
                base.Draw(gr, showSelection);
                Rectangle r = DisplayRectangle.Grow(-4);
                control.Location = r.Location;
                control.Size = r.Size;
                control.Text = Text;
                control.Enabled = Enabled;
            }
        }
    }

    [ToolboxShape]
    public class ToolboxButtonShape : GraphicElement
    {
        public const string TOOLBOX_TEXT = "btn";

        protected Brush brush = new SolidBrush(Color.Black);

        public ToolboxButtonShape(Canvas canvas) : base(canvas)
        {
            TextFont.Dispose();
            TextFont = new Font(FontFamily.GenericSansSerif, 8);
        }

        public override GraphicElement CloneDefault(Canvas canvas)
        {
            return CloneDefault(canvas, Point.Empty);
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
            ButtonShape shape = new ButtonShape(canvas);
            shape.DisplayRectangle = shape.DefaultRectangle().Move(offset);
            shape.UpdateProperties();
            shape.UpdatePath();

            return shape;
        }

        public override void Draw(Graphics gr, bool showSelection = true)
        {
            SizeF size = gr.MeasureString(TOOLBOX_TEXT, TextFont);
            Point textpos = DisplayRectangle.Center().Move((int)(-size.Width / 2), (int)(-size.Height / 2));
            gr.DrawString(TOOLBOX_TEXT, TextFont, brush, textpos);
            base.Draw(gr, showSelection);
        }
    }

}
