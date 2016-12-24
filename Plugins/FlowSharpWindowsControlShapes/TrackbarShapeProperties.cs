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
    public class TrackbarShapeProperties : ControlShapeProperties
    {
        [Category("Range")]
        public int Minimum { get; set; }
        [Category("Range")]
        public int Maximum { get; set; }
        [Category("Events")]
        public string ValueChangedName { get; set; }

        public TrackbarShapeProperties(GraphicElement el) : base(el)
        {
            Minimum = ((TrackbarShape)el).Minimum;
            Maximum = ((TrackbarShape)el).Maximum;
            ValueChangedName = ((TrackbarShape)el).ValueChangedName;
        }

        public override void Update(GraphicElement el, string label)
        {
            base.Update(el, label);
            (label == nameof(Minimum)).If(() => ((TrackbarShape)el).Minimum = Minimum);
            (label == nameof(Maximum)).If(() => ((TrackbarShape)el).Maximum = Maximum);
            (label == nameof(ValueChangedName)).If(() => ((TrackbarShape)el).ValueChangedName = ValueChangedName);
        }
    }
}
