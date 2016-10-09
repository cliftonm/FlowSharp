/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using FlowSharpLib;

namespace FlowSharp
{
	public class ToolboxController : BaseController
	{
		protected CanvasController canvasController;
        protected int xDisplacement = 0;

		public ToolboxController(Canvas canvas, List<GraphicElement> elements, CanvasController canvasController) : base(canvas, elements)
		{
			this.canvasController = canvasController;
			canvas.PaintComplete = CanvasPaintComplete;
			canvas.MouseDown += OnMouseDown;
		}

        public void ResetDisplacement()
        {
            xDisplacement = 0;
        }

		public void OnMouseDown(object sender, MouseEventArgs args)
		{
			if (args.Button == MouseButtons.Left)
			{
				selectedElement = SelectElement(args.Location);

				if (selectedElement != null)
				{
					GraphicElement el = selectedElement.CloneDefault(canvasController.Canvas, new Point(xDisplacement, 0));
                    xDisplacement += 80;
					canvasController.Insert(el);
                    canvasController.SelectElement(el);
				}
			}
		}

		protected GraphicElement SelectElement(Point p)
		{
			GraphicElement el = elements.FirstOrDefault(e => e.DisplayRectangle.Contains(p));

			return el;
		}
	}
}
