/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

using Clifton.Core.ExtensionMethods;

using FlowSharpLib;

namespace FlowSharpCodeShapes
{
    public class SemanticInstance : Box
    {
        public SemanticInstance(Canvas canvas) : base(canvas)
        {
            TextFont.Dispose();
            TextFont = new Font(FontFamily.GenericSansSerif, 6);
            TextAlign = ContentAlignment.TopCenter;
            Text = "ST";
        }

        public override GraphicElement CloneDefault(Canvas canvas)
        {
            GraphicElement el = base.CloneDefault(canvas);
            el.TextFont.Dispose();
            el.TextFont = new Font(FontFamily.GenericSansSerif, 10);
            el.TextAlign = ContentAlignment.MiddleCenter;

            return el;
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
            GraphicElement el = base.CloneDefault(canvas, offset);
            el.TextFont.Dispose();
            el.TextFont = new Font(FontFamily.GenericSansSerif, 10);
            el.TextAlign = ContentAlignment.MiddleCenter;

            return el;
        }

        public override ElementProperties CreateProperties()
        {
            SemanticInstanceProperties props = new SemanticInstanceProperties(this);
            props.SemanticType = Text;

            return props;
        }

        public override void Serialize(ElementPropertyBag epb, IEnumerable<GraphicElement> elementsBeingSerialized)
        {
            // TODO: Use JSON dictionary instead.
            base.Serialize(epb, elementsBeingSerialized);
        }

        public override void Deserialize(ElementPropertyBag epb)
        {
            // TODO: Use JSON dictionary instead.
            base.Deserialize(epb);
        }
    }

    public class SemanticInstanceProperties : ElementProperties
    {
        [Category("Assembly")]
        public string SemanticType { get; set; }

        public SemanticInstanceProperties(SemanticInstance el) : base(el)
        {
        }

        public override void UpdateFrom(GraphicElement el)
        {
            SemanticType = el.Text;
            base.UpdateFrom(el);
        }

        public override void Update(GraphicElement el, string label)
        {
            SemanticInstance si = (SemanticInstance)el;

            (label == "SemanticType").If(() =>
            {
                si.Text = string.IsNullOrEmpty(SemanticType) ? "ST" : (SemanticType);
            });

            base.Update(el, label);
        }
    }
}
