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
using FlowSharpCodeShapeInterfaces;

namespace FlowSharpCodeShapes
{
    public class AssemblyBox : Box, IAssemblyBox
    {
        public string Filename { get; set; }

        public AssemblyBox(Canvas canvas) : base(canvas)
        {
            Text = "Assy";
            TextFont.Dispose();
            TextFont= new Font(FontFamily.GenericSansSerif, 6);
            TextAlign = ContentAlignment.TopCenter;
        }

        public override ElementProperties CreateProperties()
        {
            return new AssemblyBoxProperties(this);
        }

        public override void Serialize(ElementPropertyBag epb, IEnumerable<GraphicElement> elementsBeingSerialized)
        {
            // TODO: Use JSON dictionary instead.
            epb.ExtraData = Filename;
            base.Serialize(epb, elementsBeingSerialized);
        }

        public override void Deserialize(ElementPropertyBag epb)
        {
            // TODO: Use JSON dictionary instead.
            Filename = epb.ExtraData;
            base.Deserialize(epb);
        }
    }

    public class AssemblyBoxProperties : ElementProperties
    {
        [Category("Assembly")]
        public string Filename { get; set; }

        public AssemblyBoxProperties(AssemblyBox el) : base(el)
        {
            Filename = el.Filename;
        }

        public override void Update(GraphicElement el, string label)
        {
            AssemblyBox box = (AssemblyBox)el;

            (label == "Filename").If(() =>
              {
                  box.Filename = Filename;
                  box.Text = string.IsNullOrEmpty(Filename) ? "Assy" : ("Assy: " + Filename);
              });
            
            base.Update(el, label);
        }
    }
}
