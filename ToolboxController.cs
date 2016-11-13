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
        protected Point currentDragPosition;
        protected bool setup;
        protected bool dragging;

		public ToolboxController(Canvas canvas, CanvasController canvasController) : base(canvas)
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
                GraphicElement selectedElement = GetSelectedElement(args.Location);
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
                    int where = xDisplacement;
                    xDisplacement += 80;
                    // For undo, we need to preserve currently selected shapes.
                    List<GraphicElement> currentSelectedShapes = canvasController.SelectedElements.ToList();
                    GraphicElement selectedElement = selectedElements[0];
                    GraphicElement el = selectedElement.CloneDefault(canvasController.Canvas, new Point(where, 0));

                    canvasController.UndoStack.UndoRedo("Create " + selectedElement.ToString(),
                        () =>
                        {
                            ElementCache.Instance.Remove(el);
                            el.UpdatePath();
                            canvasController.Insert(el);
                            canvasController.DeselectCurrentSelectedElements();
                            canvasController.SelectElement(el);
                        },
                        () =>
                        {
                            ElementCache.Instance.Add(el);
                            canvasController.DeselectCurrentSelectedElements();
                            canvasController.DeleteElement(el, false);
                            canvasController.SelectElements(currentSelectedShapes);
                        });
                }
            }
            else if (args.Button == MouseButtons.Left && dragging)
            {
                // X1
                // canvasController.UndoStack.FinishGroup();
            }

            dragging = false;
            mouseDown = false;
            canvasController.HideConnectionPoints();
            DeselectCurrentSelectedElement();
            selectedElements.Clear();
            canvas.Cursor = Cursors.Arrow;
        }

        public void OnMouseMove(object sender, MouseEventArgs args)
        {
            if (selectedElements.Count > 0 && mouseDown && selectedElements[0] != null && !dragging)
            {
                Point delta = args.Location.Delta(mouseDownPosition);

                if ((delta.X.Abs() > MIN_DRAG) || (delta.Y.Abs() > MIN_DRAG))
                {
                    dragging = true;
                    setup = true;
                    canvasController.DeselectCurrentSelectedElements();
                    ResetDisplacement();
                    GraphicElement el = selectedElements[0].CloneDefault(canvasController.Canvas);

                    if (el is DynamicConnector)
                    {
                        el.ShowAnchors = true;
                    }

                    Cursor.Position = canvas.PointToScreen(el.DisplayRectangle.Center().Move(canvas.Width, 0));
                    canvasController.Insert(el);
                    canvasController.SelectElement(el);
                    canvas.Cursor = Cursors.SizeAll;
                }
            }
            else if (mouseDown && selectedElements.Any() && dragging)
            {
                // First time event is because we've changed the mouse position.  Reset the current drag position so
                // we get the current mouse position, then clear the flag so drag operations continue to move the shape
                // after our mouse coordinate management is set up correctly.
                if (setup)
                {
                    currentDragPosition = args.Location;
                    setup = false;
                }
                else
                {
                    // Toolbox controller still has control, so simulate dragging on the canvas.
                    Point delta = args.Location.Delta(currentDragPosition);
                    canvasController.DragSelectedElements(delta);
                    currentDragPosition = args.Location;
                }
            }
        }

        public override void SelectElement(GraphicElement el)
        {
            DeselectCurrentSelectedElement();

            if (el != null)
            {
                var els = EraseIntersectionsTopToBottom(el);
                el.Select();
                DrawBottomToTop(els);
                UpdateScreen(els);
                selectedElements.Add(el);
                ElementSelected.Fire(this, new ElementEventArgs() { Element = el });
            }
        }

        protected GraphicElement GetSelectedElement(Point p)
		{
			GraphicElement el = elements.FirstOrDefault(e => e.DisplayRectangle.Contains(p));

			return el;
		}

        protected void DeselectCurrentSelectedElement()
        {
            if (selectedElements.Any())
            {
                var els = EraseIntersectionsTopToBottom(selectedElements[0]);
                selectedElements[0].Deselect();
                DrawBottomToTop(els);
                UpdateScreen(els);
                selectedElements.Clear();
            }
        }
    }
}
