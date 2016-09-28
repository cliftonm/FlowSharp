using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FlowSharpLib
{
	/// <summary>
	/// Special rendering for this element in the toolbox only.
	/// </summary>
	public class ToolboxDynamicConnectorLR : GraphicElement
	{
		public ToolboxDynamicConnectorLR(Canvas canvas) : base(canvas)
		{
		}

		public override GraphicElement Clone(Canvas canvas)
		{
			// Create an actual dynamic connector as this is being cloned from the toolbox.
			DynamicConnectorLR dc = new DynamicConnectorLR(canvas, new Point(20, 20), new Point(60, 60));

			return dc;
		}

		protected override void Draw(Graphics gr)
		{
			gr.DrawLine(BorderPen, DisplayRectangle.TopLeftCorner(), DisplayRectangle.TopMiddle());
			gr.DrawLine(BorderPen, DisplayRectangle.TopMiddle(), DisplayRectangle.BottomMiddle());
			gr.DrawLine(BorderPen, DisplayRectangle.BottomMiddle(), DisplayRectangle.BottomRightCorner());

			base.Draw(gr);
		}
	}

	/// <summary>
	/// Special rendering for this element in the toolbox only.
	/// </summary>
	public class ToolboxDynamicConnectorUD : GraphicElement
	{
		public ToolboxDynamicConnectorUD(Canvas canvas) : base(canvas)
		{
		}

		public override GraphicElement Clone(Canvas canvas)
		{
			// Create an actual dynamic connector as this is being cloned from the toolbox.
			DynamicConnectorUD dc = new DynamicConnectorUD(canvas, new Point(20, 20), new Point(60, 60));

			return dc;
		}

		protected override void Draw(Graphics gr)
		{
			gr.DrawLine(BorderPen, DisplayRectangle.TopLeftCorner(), DisplayRectangle.LeftMiddle());
			gr.DrawLine(BorderPen, DisplayRectangle.LeftMiddle(), DisplayRectangle.RightMiddle());
			gr.DrawLine(BorderPen, DisplayRectangle.RightMiddle(), DisplayRectangle.BottomRightCorner());

			base.Draw(gr);
		}
	}

}
