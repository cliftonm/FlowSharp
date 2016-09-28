using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using FlowSharpLib;

namespace FlowSharp
{
	public class ToolboxController : BaseController
	{
		// protected bool dragging;
		protected CanvasController canvasController;

		public ToolboxController(Canvas canvas, List<GraphicElement> elements, CanvasController canvasController) : base(canvas, elements)
		{
			this.canvasController = canvasController;
			canvas.PaintComplete = CanvasPaintComplete;
			canvas.MouseDown += OnMouseDown;
//			canvas.MouseUp += OnMouseUp;
//			canvas.MouseMove += OnMouseMove;
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
				}
				// dragging = selectedElement != null;
			}
		}
/*
		public void OnMouseUp(object sender, MouseEventArgs args)
		{
			if (args.Button == MouseButtons.Left)
			{
				selectedElement = null;
				dragging = false;
			}
		}

		public void OnMouseMove(object sender, MouseEventArgs args)
		{
			if (dragging)
			{
				if (args.Location.X - 10 > canvas.Width)
				{
					GraphicElement el = selectedElement.Clone();
					el.DisplayRectangle = new Rectangle(20, args.Location.Y - 20, 40, 40);
					canvasController.Insert(el);					
				}
			}
		}
*/
		protected GraphicElement SelectElement(Point p)
		{
			GraphicElement el = elements.FirstOrDefault(e => e.DisplayRectangle.Contains(p));

			return el;
		}
	}
}
