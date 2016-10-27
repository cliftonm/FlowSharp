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

		public override void Update(GraphicElement el)
		{
			((DynamicConnector)el).StartCap = StartCap;
			((DynamicConnector)el).EndCap = EndCap;
			base.Update(el);
		}
	}
}
