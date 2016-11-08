/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.ComponentModel;
using System.Drawing;

namespace FlowSharpLib
{
    public class ShapeProperties : ElementProperties
    {
        [Category("Text")]
        public string Text { get; set; }
        [Category("Text")]
        public Font Font { get; set; }
        [Category("Text")]
        public Color TextColor { get; set; }
        [Category("Text")]
        public ContentAlignment TextAlign { get; set; }

        public ShapeProperties(GraphicElement el) : base(el)
        {
            Text = el.Text;
            Font = el.TextFont;
            TextColor = el.TextColor;
            TextAlign = el.TextAlign;
        }

        public override void Update(GraphicElement el, string label)
        {
            (label == "Text").If(() => this.ChangePropertyWithUndoRedo<string>(el, "Text", "Text"));
            (label == "Font").If(() => this.ChangePropertyWithUndoRedo<Font>(el, "TextFont", "Font"));
            (label == "TextColor").If(() => this.ChangePropertyWithUndoRedo<Color>(el, "TextColor", "TextColor"));
            (label == "TextAlign").If(() => this.ChangePropertyWithUndoRedo<Color>(el, "TextAlign", "TextAlign"));
            base.Update(el, label);
        }
    }
}
