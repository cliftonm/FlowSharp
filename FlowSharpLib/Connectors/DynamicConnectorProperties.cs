/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.ComponentModel;

namespace FlowSharpLib
{
	public class DynamicConnectorProperties : ElementProperties
	{
		[Category("Endcaps")]
		public AvailableLineCap StartCap { get; set; }
		[Category("Endcaps")]
		public AvailableLineCap EndCap { get; set; }

		public DynamicConnectorProperties(DynamicConnector el) : base(el)
		{
			StartCap = el.StartCap;
			EndCap = el.EndCap;
		}

		public override void Update(GraphicElement el, string label)
		{
            // X1
            //(label == nameof(StartCap)).If(()=> this.ChangePropertyWithUndoRedo<AvailableLineCap>(el, nameof(StartCap), nameof(StartCap)));
            //(label == nameof(StartCap)).If(() => this.ChangePropertyWithUndoRedo<AvailableLineCap>(el, nameof(EndCap), nameof(EndCap)));
            (label == nameof(StartCap)).If(() => ((Connector)el).StartCap = StartCap);
            (label == nameof(StartCap)).If(() => ((Connector)el).EndCap = EndCap);
            base.Update(el, label);
		}
	}
}
