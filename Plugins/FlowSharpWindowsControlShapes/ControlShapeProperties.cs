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
    public class ControlShapeProperties : ShapeProperties
    {
        [Category("Click")]
        public string ClickEventName { get; set; }
        [Category("Click")]
        public string ClickEventData { get; set; }

        [Category("Visual")]
        public bool Enabled { get; set; }
        [Category("Visual")]
        public bool Visible { get; set; }

        public ControlShapeProperties(GraphicElement el) : base(el)
        {
            ClickEventName = ((ControlShape)el).ClickEventName;
            ClickEventData = ((ControlShape)el).ClickEventData;
            Enabled = ((ControlShape)el).Enabled;
            Visible = ((ControlShape)el).Visible;
        }

        public override void Update(GraphicElement el, string label)
        {
            base.Update(el, label);
            (label == nameof(ClickEventName)).If(() => ((ControlShape)el).ClickEventName = ClickEventName);
            (label == nameof(ClickEventData)).If(() => ((ControlShape)el).ClickEventData = ClickEventData);
            (label == nameof(Enabled)).If(() => ((ControlShape)el).Enabled = Enabled);
            (label == nameof(Visible)).If(() => ((ControlShape)el).Visible = Visible);
        }
    }
}
