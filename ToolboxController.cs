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
        public const int MIN_DRAG = 3;

		protected CanvasController canvasController;
        protected int xDisplacement = 0;
        protected bool mouseDown = false;
        protected Point mouseDownPosition;

		public ToolboxController(Canvas canvas, List<GraphicElement> elements, CanvasController canvasController) : base(canvas, elements)
		{
			this.canvasController = canvasController;
			canvas.PaintComplete = CanvasPaintComplete;
			canvas.MouseClick += OnMouseClick;
            canvas.MouseDown += OnMouseDown;
            canvas.MouseUp += OnMouseUp;
            canvas.MouseMove += OnMouseMove;
        }

        public void ResetDisplacement()
        {
            xDisplacement = 0;
        }

		public void OnMouseClick(object sender, MouseEventArgs args)
		{
		}

        public void OnMouseDown(object sender, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                selectedElement = SelectElement(args.Location);
                mouseDown = true;
                mouseDownPosition = args.Location;
            }
        }

        public void OnMouseUp(object sender, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left && !dragging)
            {
                if (selectedElement != null)
                {
                    GraphicElement el = selectedElement.CloneDefault(canvasController.Canvas, new Point(xDisplacement, 0));
                    xDisplacement += 80;
                    canvasController.Insert(el);
                    canvasController.SelectElement(el);
                }
            }

            dragging = false;
            mouseDown = false;
            canvasController.EndDraggingMode();
        }

        public void OnMouseMove(object sender, MouseEventArgs args)
        {
            if (mouseDown && selectedElement != null && !dragging)
            {
                Point delta = args.Location.Delta(mouseDownPosition);

                if ((delta.X.Abs() > MIN_DRAG) || (delta.Y.Abs() > MIN_DRAG))
                {
                    dragging = true;
                    ResetDisplacement();
                    Point screenPos = new Point(canvas.Width, args.Location.Y);
                    Point canvasPos = new Point(0, args.Location.Y);
                    Point p = canvas.PointToScreen(screenPos);
                    Cursor.Position = p;

                    GraphicElement el = selectedElement.CloneDefault(canvasController.Canvas);
                    canvasController.Insert(el);
                    Point offset = new Point(-el.DisplayRectangle.X - el.DisplayRectangle.Width/2 - 5, -el.DisplayRectangle.Y + args.Location.Y - el.DisplayRectangle.Height / 2);

                    // TODO: Why this fudge factor for DC's?
                    if (el is DynamicConnector)
                    {
                        offset = offset.Move(8, 6);
                    }

                    canvasController.MoveElement(el, offset);
                    canvasController.StartDraggingMode(el, canvasPos);
                    canvasController.SelectElement(el);
                }
            }
            else if (mouseDown && selectedElement != null && dragging)
            {
                // Toolbox controller still has control, so simulate dragging on the canvas.
                Point p = new Point(args.Location.X - canvas.Width, args.Location.Y);
                canvasController.DragShape(p);
            }
        }

        protected GraphicElement SelectElement(Point p)
		{
			GraphicElement el = elements.FirstOrDefault(e => e.DisplayRectangle.Contains(p));

			return el;
		}
	}
}
