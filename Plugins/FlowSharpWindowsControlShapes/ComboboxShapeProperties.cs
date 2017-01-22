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
    public class ComboboxShapeProperties : ControlShapeProperties
    {
        [Category("Fields")]
        public string IdFieldName { get; set; }
        [Category("Fields")]
        public string DisplayFieldName { get; set; }

        public ComboboxShapeProperties(GraphicElement el) : base(el)
        {
            IdFieldName = ((ComboboxShape)el).IdFieldName;
            DisplayFieldName = ((ComboboxShape)el).DisplayFieldName;
        }

        public override void Update(GraphicElement el, string label)
        {
            base.Update(el, label);
            (label == nameof(IdFieldName)).If(() => ((ComboboxShape)el).IdFieldName = IdFieldName);
            (label == nameof(DisplayFieldName)).If(() => ((ComboboxShape)el).DisplayFieldName = DisplayFieldName);
        }
    }
}
