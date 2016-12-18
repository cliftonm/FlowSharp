/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;

using FlowSharpLib;

namespace FlowSharpWindowsControlShapes
{
    public class TextboxShape : ControlShape
    {
        public bool Multiline { get; set; }

        public TextboxShape(Canvas canvas) : base(canvas)
        {
            control = new TextBox();
            canvas.Controls.Add(control);
        }

        public override ElementProperties CreateProperties()
        {
            return new TextboxShapeProperties(this);
        }

        public override void Serialize(ElementPropertyBag epb, IEnumerable<GraphicElement> elementsBeingSerialized)
        {
            Json["Multiline"] = Multiline.ToString();
            base.Serialize(epb, elementsBeingSerialized);
        }

        public override void Deserialize(ElementPropertyBag epb)
        {
            base.Deserialize(epb);
            string multiline;

            if (Json.TryGetValue("Multiline", out multiline))
            {
                Multiline = multiline.to_b();
            }
        }

        public override void Draw(Graphics gr)
        {
            base.Draw(gr);
            Rectangle r = DisplayRectangle.Grow(-4);
            control.Location = r.Location;
            control.Size = r.Size;
            control.Text = Text;
            control.Font = TextFont;
            ((TextBox)control).Multiline = Multiline;
        }

        public override void DrawText(Graphics gr)
        {
            // Do nothing, as we don't display the text in the shape itself, only the edit control.
        }
    }
}
