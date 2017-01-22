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
    public class ListBoxShapeProperties : ControlShapeProperties
    {
        [Category("Fields")]
        public string IdFieldName { get; set; }
        [Category("Fields")]
        public string DisplayFieldName { get; set; }

        public ListBoxShapeProperties(GraphicElement el) : base(el)
        {
            IdFieldName = ((ListBoxShape)el).IdFieldName;
            DisplayFieldName = ((ListBoxShape)el).DisplayFieldName;
        }

        public override void Update(GraphicElement el, string label)
        {
            base.Update(el, label);
            (label == nameof(IdFieldName)).If(() => ((ListBoxShape)el).IdFieldName = IdFieldName);
            (label == nameof(DisplayFieldName)).If(() => ((ListBoxShape)el).DisplayFieldName = DisplayFieldName);
        }
    }
}
