/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.ComponentModel;

using Clifton.Core.ExtensionMethods;

using FlowSharpLib;

namespace PluginExample
{
    public class NavToProperties : ElementProperties
    {
        [Category("Navigate")]
        [Description("Navigate to the shape with the specified 'Name' (not the ShapeName)")]
        public string To { get; set; }

        public NavToProperties(NavTo el) : base(el)
        {
            To = el.NavigateTo;
        }

        public override void Update(GraphicElement el, string label)
        {
            (label == nameof(To)).If(() => ((NavTo)el).NavigateTo = To);
            base.Update(el, label);
        }
    }
}