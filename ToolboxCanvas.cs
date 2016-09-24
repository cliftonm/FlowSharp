using System.Drawing;

using FlowSharpLib;

namespace FlowSharp
{
	public class ToolboxCanvas : Canvas
	{
		protected override void DrawBackground(Graphics gr)
		{
			gr.Clear(Color.LightGray);
		}

		protected override void DrawGrid(Graphics gr)
		{
		}
	}
}
