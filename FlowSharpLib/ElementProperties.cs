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

		public LineProperties(GraphicElement el) : base(el)
		{
		}

		public override void Update(GraphicElement el)
		{
			base.Update(el);
			((HorizontalLine)el).StartCap = StartCap;
			((HorizontalLine)el).EndCap = EndCap;
		}
	}
}
