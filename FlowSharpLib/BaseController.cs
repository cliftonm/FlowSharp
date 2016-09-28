using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace FlowSharpLib
{
	public abstract class BaseController
	{
		public const int MIN_WIDTH = 20;
		public const int MIN_HEIGHT = 20;

		public const int SNAP_ELEMENT_RANGE = 20;
		public const int SNAP_CONNECTION_POINT_RANGE = 10;
		public const int SNAP_DETACH_VELOCITY = 5;

		public const int CONNECTION_POINT_SIZE = 3;		// this is actually the length from center.

		public Canvas Canvas { get { return canvas; } }

		protected List<GraphicElement> elements;
		public EventHandler<ElementEventArgs> ElementSelected;
		public EventHandler<ElementEventArgs> UpdateSelectedElement;

		public GraphicElement SelectedElement { get { return selectedElement; } }

		protected Canvas canvas;
		protected GraphicElement selectedElement;
		protected ShapeAnchor selectedAnchor;

		public BaseController(Canvas canvas, List<GraphicElement> elements)
		{
			this.canvas = canvas;
			this.elements = elements;
		}

		public virtual bool Snap(GripType type, ref Point delta) { return false; }

		public void Redraw(GraphicElement el, int dx=0, int dy=0)
		{
			var els = EraseTopToBottom(el, dx, dy);
			DrawBottomToTop(els, dx, dy);
			UpdateScreen(els, dx, dy);
		}

		public void Redraw(GraphicElement el, Action<GraphicElement> afterErase)
		{
			var els = EraseTopToBottom(el);
			UpdateScreen(els);
			afterErase(el);
			DrawBottomToTop(els);
			UpdateScreen(els);
		}

		public void Insert(GraphicElement el)
		{
			elements.Insert(0, el);
			Redraw(el);
		}

		public void UpdateSize(GraphicElement el, ShapeAnchor anchor, Point delta)
		{
			Point adjustedDelta = anchor.AdjustedDelta(delta);
			Rectangle newRect = anchor.Resize(el.DisplayRectangle, adjustedDelta);
			UpdateDisplayRectangle(el, newRect, adjustedDelta);
			UpdateConnections(el);
		}

		/// <summary>
		/// Direct update of display rectangle, used in DynamicConnector.
		/// </summary>
		public void UpdateDisplayRectangle(GraphicElement el, Rectangle newRect, Point delta)
		{
			int dx = delta.X.Abs();
			int dy = delta.Y.Abs();
			List<GraphicElement> els = EraseTopToBottom(el, dx, dy);
			el.DisplayRectangle = newRect;
			el.UpdatePath();
			DrawBottomToTop(els, dx, dy);
			UpdateScreen(els, dx, dy);
		}

		protected void UpdateConnections(GraphicElement el)
		{
			el.Connections.ForEach(c =>
			{
				// Connection point on shape.
				ConnectionPoint cp = el.GetConnectionPoints().Single(cp2 => cp2.Type == c.ElementConnectionPoint.Type);
				c.ToElement.MoveAnchor(cp, c.ToConnectionPoint);
			});
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
				// TODO: Display element if moved back on screen at this point?
			}
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

		protected void CanvasPaintComplete(Canvas canvas)
		{
			DrawBottomToTop(elements);
		}
	}
}
