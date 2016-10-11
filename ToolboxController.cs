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
                GraphicElement selectedElement = SelectElement(args.Location);
                mouseDown = true;
                mouseDownPosition = args.Location;
                SelectElement(selectedElement);
            }
        }

        public void OnMouseUp(object sender, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left && !dragging)
            {
                if (selectedElements.Any())
                {
                    GraphicElement el = selectedElements[0].CloneDefault(canvasController.Canvas, new Point(xDisplacement, 0));
                    el.UpdatePath();
                    xDisplacement += 80;
                    canvasController.Insert(el);
                    canvasController.DeselectCurrentSelectedElements();
                    canvasController.SelectElement(el);
                }
            }

            dragging = false;
            mouseDown = false;
            canvasController.EndDraggingMode();
            DeselectCurrentSelectedElement();
            selectedElements.Clear();
            canvas.Cursor = Cursors.Arrow;
        }

        public void OnMouseMove(object sender, MouseEventArgs args)
        {
            if (mouseDown && selectedElements[0] != null && !dragging)
            {
                Point delta = args.Location.Delta(mouseDownPosition);

                if ((delta.X.Abs() > MIN_DRAG) || (delta.Y.Abs() > MIN_DRAG))
                {
                    canvasController.DeselectCurrentSelectedElements();
                    dragging = true;
                    ResetDisplacement();
                    Point screenPos = new Point(canvas.Width, args.Location.Y);     // target canvas screen position is the toolbox canvas width, toolbox mouse Y.
                    Point canvasPos = new Point(0, args.Location.Y);                // target canvas position is left edge, toolbox mouse Y.
                    Point p = canvas.PointToScreen(screenPos);                      // screen position of mouse cursor, relative to the target canvas.
                    Cursor.Position = p;

                    GraphicElement el = selectedElements[0].CloneDefault(canvasController.Canvas);
                    canvasController.Insert(el);
                    // Shape is placed so that the center of the shape is at the left edge (X), centered around the toolbox mouse (Y)
                    // The "-5" accounts for additional pixels between the toolbox end and the canvas start, should be calculable by converting toolbox canvas width to screen coordinate and subtracting
                    // that from the target canvas left edge screen coordinate.
                    Point offset = new Point(-el.DisplayRectangle.X - el.DisplayRectangle.Width/2 - 5, -el.DisplayRectangle.Y + args.Location.Y - el.DisplayRectangle.Height / 2);

                    // TODO: Why this fudge factor for DC's?
                    if (el is DynamicConnector)
                    {
                        offset = offset.Move(8, 6);
                    }

                    canvasController.MoveElement(el, offset);
                    canvasController.StartDraggingMode(el, canvasPos);
                    canvasController.SelectElement(el);
                    canvas.Cursor = Cursors.SizeAll;
                }
            }
            else if (mouseDown && selectedElements.Any() && dragging)
            {
                // Toolbox controller still has control, so simulate dragging on the canvas.
                Point p = new Point(args.Location.X - canvas.Width, args.Location.Y);
                canvasController.DragShape(p);
            }
        }

        public override void SelectElement(GraphicElement el)
        {
            DeselectCurrentSelectedElement();

            if (el != null)
            {
                var els = EraseTopToBottom(el);
                el.Selected = true;
                DrawBottomToTop(els);
                UpdateScreen(els);
                selectedElements.Add(el);
                ElementSelected.Fire(this, new ElementEventArgs() { Element = el });
            }
        }

        protected GraphicElement SelectElement(Point p)
		{
			GraphicElement el = elements.FirstOrDefault(e => e.DisplayRectangle.Contains(p));

			return el;
		}

        protected void DeselectCurrentSelectedElement()
        {
            if (selectedElements.Any())
            {
                var els = EraseTopToBottom(selectedElements[0]);
                selectedElements[0].Selected = false;
                DrawBottomToTop(els);
                UpdateScreen(els);
                selectedElements.Clear();
            }
        }
    }
}
