using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FlowSharpLib
{
	public class DynamicConnectorProperties : ElementProperties
	{
		[Category("Endcaps")]
		public AvailableLineCap StartCap { get; set; }
		[Category("Endcaps")]
		public AvailableLineCap EndCap { get; set; }

		public DynamicConnectorProperties(IDynamicConnector el) : base((GraphicElement)el)
		{
			StartCap = el.StartCap;
			EndCap = el.EndCap;
		}

		public override void Update(GraphicElement el)
		{
			((DynamicConnectorLR)el).StartCap = StartCap;
			((DynamicConnectorLR)el).EndCap = EndCap;
			base.Update(el);
		}
	}
}
