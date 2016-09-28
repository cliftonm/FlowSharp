using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FlowSharpLib
{
	public class LineProperties : ElementProperties
	{
		[Category("Endcaps")]
		public AvailableLineCap StartCap { get; set; }
		[Category("Endcaps")]
		public AvailableLineCap EndCap { get; set; }

		public LineProperties(Line el) : base((GraphicElement)el)
		{
			StartCap = el.StartCap;
			EndCap = el.EndCap;
		}

		public override void Update(GraphicElement el)
		{
			base.Update(el);
			((Line)el).StartCap = StartCap;
			((Line)el).EndCap = EndCap;
		}
	}
}