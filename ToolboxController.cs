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

		public ToolboxController(Canvas canvas, List<GraphicElement> elements, CanvasController canvasController) : base(canvas, elements)
		{
			this.canvasController = canvasController;
			canvas.PaintComplete = CanvasPaintComplete;
			canvas.MouseDown += OnMouseDown;
		}

		public void OnMouseDown(object sender, MouseEventArgs args)
		{
			if (args.Button == MouseButtons.Left)
			{
				selectedElement = SelectElement(args.Location);

				if (selectedElement != null)
				{
					GraphicElement el = selectedElement.Clone(canvasController.Canvas);
					el.DisplayRectangle = el.DefaultRectangle();
					el.UpdatePath();
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
