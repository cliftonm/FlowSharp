using System.Drawing;

namespace FlowSharpLib
{
	public interface IDynamicConnector
	{
		AvailableLineCap StartCap { get; set; }
		AvailableLineCap EndCap { get; set; }
	}
}
