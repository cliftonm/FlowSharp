/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

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
