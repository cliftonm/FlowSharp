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

		public LineProperties(ILine el) : base((GraphicElement)el)
		{
			StartCap = el.StartCap;
			EndCap = el.EndCap;
		}

		public override void Update(GraphicElement el)
		{
			base.Update(el);
			((ILine)el).StartCap = StartCap;
			((ILine)el).EndCap = EndCap;
		}
	}
}