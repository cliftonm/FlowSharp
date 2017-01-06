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
            control.Enabled = Enabled;
            control.Visible = Visible;
        }
    }

    [ToolboxShape]
    public class ToolboxCheckboxShape : GraphicElement
    {
        public const string TOOLBOX_TEXT = "ckbox";

        protected Brush brush = new SolidBrush(Color.Black);

        public ToolboxCheckboxShape(Canvas canvas) : base(canvas)
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
            CheckboxShape shape = new CheckboxShape(canvas);
            shape.DisplayRectangle = shape.DefaultRectangle().Move(offset);
            shape.UpdateProperties();
            shape.UpdatePath();

            return shape;
        }

        public override void Draw(Graphics gr)
        {
            SizeF size = gr.MeasureString(TOOLBOX_TEXT, TextFont);
            Point textpos = DisplayRectangle.Center().Move((int)(-size.Width / 2), (int)(-size.Height / 2));
            gr.DrawString(TOOLBOX_TEXT, TextFont, brush, textpos);
            base.Draw(gr);
        }
    }
}
