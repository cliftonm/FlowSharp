using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FlowSharp
{
	public class ElementEventArgs : EventArgs
	{
		public GraphicElement Element { get; set; }
	}

	public class CanvasController
	{
		public EventHandler<ElementEventArgs> ElementSelected;
		public EventHandler<ElementEventArgs> UpdateSelectedElement;

		public const int MIN_WIDTH = 20;
		public const int MIN_HEIGHT = 20;
		public GraphicElement SelectedElement { get { return selectedElement; } }

		protected Canvas canvas;
		protected List<GraphicElement> elements;
		protected bool dragging;
		protected bool leftMouseDown;
		protected GraphicElement selectedElement;
		protected Anchor selectedAnchor;
		protected GraphicElement showingAnchorsElement;
		protected Point mousePosition;
		
		public CanvasController(Canvas canvas, List<GraphicElement> elements)
		{
			this.elements = elements;
			canvas.PaintComplete = SurfacePaintComplete;
			canvas.MouseDown += (sndr, args) =>
			{
				leftMouseDown = true;
				DeselectCurrentSelectedElement();
				SelectElement(args.Location);
				selectedAnchor = selectedElement?.GetAnchors().FirstOrDefault(a => a.Near(mousePosition));
				ElementSelected.Fire(this, new ElementEventArgs() { Element = selectedElement });
				dragging = args.Button == MouseButtons.Left && selectedElement != null;
				mousePosition = args.Location;
			};

			canvas.MouseUp += (sndr, args) =>
			{
				selectedAnchor = null;
				leftMouseDown = false;
				dragging = !(args.Button == MouseButtons.Left);
			};

			canvas.MouseMove += OnMouseMove;
		}

		public void Redraw(GraphicElement el)
		{
			var els = EraseTopToBottom(el);
			DrawBottomToTop(els);
			UpdateScreen(els);
		}

		public void Redraw(GraphicElement el, Action<GraphicElement> afterErase)
		{
			var els = EraseTopToBottom(el);
			UpdateScreen(els);
			afterErase(el);
			DrawBottomToTop(els);
			UpdateScreen(els);
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

		protected void MoveElement(GraphicElement el, Point delta)
		{
			if (el.OnScreen())
			{
				List<GraphicElement> els = EraseTopToBottom(el, delta.X.Abs(), delta.Y.Abs());
				el.Move(delta);
				el.UpdatePath();
				int dx = delta.X.Abs();
				int dy = delta.Y.Abs();
				DrawBottomToTop(els, dx, dy);
				UpdateScreen(els, dx, dy);
			}
			else
			{
				el.CancelBackground();
				el.Move(delta);
			}
		}

		protected void UpdateSize(GraphicElement el, Anchor anchor, Point delta)
		{
			Point adjustedDelta = anchor.AdjustedDelta(delta);
			Rectangle newRect = anchor.Resize(el.DisplayRectangle, adjustedDelta);

			// Don't get too small.
			if (newRect.Width > MIN_WIDTH && newRect.Height > MIN_HEIGHT)
			{
				List<GraphicElement> els = EraseTopToBottom(el, adjustedDelta.X.Abs(), adjustedDelta.Y.Abs());
				el.DisplayRectangle = newRect;
				el.UpdatePath();
				int dx = delta.X.Abs();
				int dy = delta.Y.Abs();
				DrawBottomToTop(els, dx, dy);
				UpdateScreen(els, dx, dy);
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

		/// <summary>
		/// Recursive loop to get all intersecting rectangles, including intersectors of the intersectees, so that all elements that
		/// are affected by an overlap redraw are erased and redrawn, otherwise we get artifacts of some intersecting elements when intersection count > 2.
		/// </summary>
		protected void FindAllIntersections(List<GraphicElement> intersections, GraphicElement el, int dx = 0, int dy = 0)
		{
			// Cool thing here is that if the element has no intersections, this list still returns that element because it intersects with itself!
			elements.Where(e => !intersections.Contains(e) && e.UpdateRectangle.IntersectsWith(el.UpdateRectangle.Grow(dx, dy))).ForEach((e) =>
			{
				intersections.Add(e);
				FindAllIntersections(intersections, e);
			});
		}

		protected List<GraphicElement> EraseTopToBottom(GraphicElement el, int dx = 0, int dy = 0)
		{
			List<GraphicElement> intersections = new List<GraphicElement>();
			FindAllIntersections(intersections, el, dx, dy);
			List<GraphicElement> els = intersections.OrderBy(e => elements.IndexOf(e)).ToList();
			els.Where(e => e.OnScreen(dx, dy)).ForEach(e => e.Erase());

			return els;
		}

		protected void DrawBottomToTop(List<GraphicElement> els, int dx = 0, int dy = 0)
		{
			// Don't modify the original list.
			els.AsEnumerable().Reverse().Where(e => e.OnScreen(dx, dy)).ForEach(e =>
			{
				e.GetBackground();
				e.Draw();
			});
		}

		protected void UpdateScreen(List<GraphicElement> els, int dx = 0, int dy = 0)
		{
			// Is this faster than creating a unioned rectangle?  Dunno, because the unioned rectangle might include a lot of space not part of the shapes, like something in an "L" pattern.
			els.Where(e => e.OnScreen(dx, dy)).ForEach(e => e.UpdateScreen(dx, dy));
		}

		protected void SurfacePaintComplete(Canvas canvas)
		{
			DrawBottomToTop(elements);
		}
	}
}