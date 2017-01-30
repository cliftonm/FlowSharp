/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.ComponentModel;

using FlowSharpLib;

using Clifton.Core.ExtensionMethods;

namespace FlowSharpWindowsControlShapes
{
    public class TextboxShapeProperties : ControlShapeProperties
    {
        [Category("Options")]
        public bool ReadOnly { get; set; }

        public TextboxShapeProperties(GraphicElement el) : base(el)
        {
            Multiline = ((TextboxShape)el).Multiline;
            ReadOnly = ((TextboxShape)el).ReadOnly;
        }

        public override void Update(GraphicElement el, string label)
        {
            base.Update(el, label);
            (label == nameof(Multiline)).If(() => ((TextboxShape)el).Multiline = Multiline);
            (label == nameof(ReadOnly)).If(() => ((TextboxShape)el).ReadOnly = ReadOnly);
        }
    }
}
