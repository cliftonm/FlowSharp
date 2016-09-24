using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FlowSharpLib
{
	public class ElementEventArgs : EventArgs
	{
		public GraphicElement Element { get; set; }
	}

	public class CanvasController : BaseController
	{
		public EventHandler<ElementEventArgs> ElementSelected;
		public EventHandler<ElementEventArgs> UpdateSelectedElement;

		public GraphicElement SelectedElement { get { return selectedElement; } }

		protected bool dragging;
		protected bool leftMouseDown;
		protected GraphicElement selectedElement;
		protected Anchor selectedAnchor;
		protected GraphicElement showingAnchorsElement;
		protected Point mousePosition;
		
		public CanvasController(Canvas canvas, List<GraphicElement> elements) : base(canvas, elements)
		{
			canvas.PaintComplete = CanvasPaintComplete;
			canvas.MouseDown += (sndr, args) =>
			{
				if (args.Button == MouseButtons.Left)
				{
					leftMouseDown = true;
					DeselectCurrentSelectedElement();
					SelectElement(args.Location);
					selectedAnchor = selectedElement?.GetAnchors().FirstOrDefault(a => a.Near(mousePosition));
					ElementSelected.Fire(this, new ElementEventArgs() { Element = selectedElement });
					dragging = selectedElement != null;
					mousePosition = args.Location;
				}
			};

			canvas.MouseUp += (sndr, args) =>
			{
				if (args.Button == MouseButtons.Left)
				{
					selectedAnchor = null;
					leftMouseDown = false;
					dragging = !(args.Button == MouseButtons.Left);
				}
			};

			canvas.MouseMove += OnMouseMove;
		}

		protected void OnMouseMove(object sender, MouseEventArgs args)
		{
			Point delta = args.Location.Delta(mousePosition);
			mousePosition = args.Location;

			if (dragging)
			{
				if (selectedAnchor != null)
				{
					UpdateSize(selectedElement, selectedAnchor, delta);
					UpdateSelectedElement(this, new ElementEventArgs() { Element = SelectedElement });
				}
				else
				{
					MoveElement(selectedElement, delta);
					UpdateSelectedElement(this, new ElementEventArgs() { Element = SelectedElement });
				}
			}
			else if (leftMouseDown)
			{
				// Pick up every object on the canvas and move it.
				// This does not "move" the grid.
				elements.ForEach(el =>
				{
					MoveElement(el, delta);
				});

				// Conversely, we redraw the grid and invalidate, which forces all the elements to redraw.
				//canvas.Drag(delta);
				//elements.ForEach(el => el.Move(delta));
				//canvas.Invalidate();
			}
			else
			{
				GraphicElement el = elements.FirstOrDefault(e => e.DisplayRectangle.Contains(args.Location));

				// Remove anchors from current object being moused over and show, if an element selected on new object.
				if (el != showingAnchorsElement)
				{
					if (showingAnchorsElement != null)
					{
						showingAnchorsElement.ShowAnchors = false;
						Redraw(showingAnchorsElement);
						showingAnchorsElement = null;
					}

					if (el != null)
					{
						el.ShowAnchors = true;
						Redraw(el);
						showingAnchorsElement = el;
					}
				}
			}
		}

		protected void DeselectCurrentSelectedElement()
		{
			if (selectedElement != null)
			{
				var els = EraseTopToBottom(selectedElement);
				selectedElement.Selected = false;
				DrawBottomToTop(els);
				UpdateScreen(els);
				selectedElement = null;
			}
		}

		protected bool SelectElement(Point p)
		{
			GraphicElement el = elements.FirstOrDefault(e => e.DisplayRectangle.Contains(p));

			if (el != null)
			{
				var els = EraseTopToBottom(el);
				el.Selected = true;
				DrawBottomToTop(els);
				UpdateScreen(els);
			}

			selectedElement = el;

			return el != null;
		}
	}
}