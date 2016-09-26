using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FlowSharpLib
{
	/// <summary>
	/// Special rendering for this element in the toolbox only.
	/// </summary>
	public class ToolboxDynamicConnector : GraphicElement
	{
		public ToolboxDynamicConnector(Canvas canvas) : base(canvas)
		{
			FillBrush = new SolidBrush(Color.White);
			BorderPen = new Pen(Color.Black);
			BorderPen.Width = 1;
		}

		public override GraphicElement Clone(Canvas canvas)
		{
			DynamicConnector line = (DynamicConnector)base.Clone(canvas);

			return line;
		}

		protected override void Draw(Graphics gr)
		{
			gr.DrawLine(BorderPen, DisplayRectangle.TopLeftCorner(), DisplayRectangle.TopMiddle());
			gr.DrawLine(BorderPen, DisplayRectangle.TopMiddle(), DisplayRectangle.BottomMiddle());
			gr.DrawLine(BorderPen, DisplayRectangle.BottomMiddle(), DisplayRectangle.BottomRightCorner());

			base.Draw(gr);
		}
	}
}
