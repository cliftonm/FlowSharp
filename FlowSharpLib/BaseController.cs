using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace FlowSharpLib
{
	public class BaseController
	{
		public const int MIN_WIDTH = 20;
		public const int MIN_HEIGHT = 20;

		public Canvas Canvas { get { return canvas; } }

		protected Canvas canvas;
		protected List<GraphicElement> elements;

		public BaseController(Canvas canvas, List<GraphicElement> elements)
		{
			this.canvas = canvas;
			this.elements = elements;
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

		public void Insert(GraphicElement el)
		{
			elements.Insert(0, el);
			Redraw(el);
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
			System.Drawing.Rectangle newRect = anchor.Resize((System.Drawing.Rectangle)el.DisplayRectangle, adjustedDelta);

			// Don't get too small, but growing from something that is too small is OK (safety check if we create an object that is too small.)
			if ( (newRect.Width >= MIN_WIDTH && newRect.Height >= MIN_HEIGHT) || (delta.X > 0 || delta.Y > 0) )
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
