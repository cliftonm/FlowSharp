/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.ComponentModel;

using FlowSharpLib;

namespace PluginExample
{
    public class ImageShapeProperties : ElementProperties
    {
        [Category("Image")]
        public string Filename { get; set; }

        public ImageShapeProperties(ImageShape el) : base(el)
        {
            Filename = el.Filename;
        }

        public override void Update(GraphicElement el)
        {
            ((ImageShape)el).Filename = Filename;
            base.Update(el);
        }
    }
}