/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        public const int CAP_WIDTH = 5;
        public const int CAP_HEIGHT = 5;

		public Canvas Canvas { get { return canvas; } }
        public ReadOnlyCollection<GraphicElement> Elements { get { return elements.AsReadOnly(); } }

		protected List<GraphicElement> elements;
		public EventHandler<ElementEventArgs> ElementSelected;
		public EventHandler<ElementEventArgs> UpdateSelectedElement;

		public List<GraphicElement> SelectedElements { get { return selectedElements; } }

		protected Canvas canvas;
		protected List<GraphicElement> selectedElements;
		protected ShapeAnchor selectedAnchor;
		protected GraphicElement showingAnchorsElement;

		protected bool dragging;
		protected bool leftMouseDown;
        protected bool rightMouseDown;
        protected bool selectionMode;

		public BaseController(Canvas canvas, List<GraphicElement> elements)
		{
			this.canvas = canvas;
			this.elements = elements;
            selectedElements = new List<GraphicElement>();
		}

		public virtual bool Snap(GripType type, ref Point delta) { return false; }
        public virtual void SelectElement(GraphicElement el) { }
        public virtual void SetAnchorCursor(GraphicElement el) { }
        public virtual void DragSelectedElements(Point delta) { }
        public virtual bool IsMultiSelect() { return false; }
        public virtual void DeselectCurrentSelectedElements() { }
        public virtual void DeselectElement(GraphicElement el) { }
        public virtual void HideConnectionPoints() { }

        public bool IsShapeSelectable(Point p)
        {
            return elements.Any(e => e.IsSelectable(p) && e.Parent == null);
        }

        public GraphicElement GetShapeAt(Point p)
        {
            return elements.FirstOrDefault(e => e.IsSelectable(p) && e.Parent == null);
        }

        public void SelectElements(List<GraphicElement> els)
        {
            els.ForEach(el => SelectElement(el));
        }

        public void Topmost()
		{
            selectedElements.ForEach(el =>
            {
                EraseTopToBottom(elements);
                elements.Remove(el);
                elements.Insert(0, el);
                DrawBottomToTop(elements);
                UpdateScreen(elements);
            });
		}

		public void Bottommost()
		{
            selectedElements.ForEach(el =>
            {
                EraseTopToBottom(elements);
                elements.Remove(el);
                elements.Add(el);
                DrawBottomToTop(elements);
                UpdateScreen(elements);
            });
		}

		public void MoveUp()
		{
            selectedElements.ForEach(el =>
            {
                int idx = elements.IndexOf(el);

                if (idx > 0)
                {
                    Reorder(el, idx - 1);
                }
            });
		}

		public void MoveDown()
		{
            selectedElements.ForEach(el =>
            {
            int idx = elements.IndexOf(el);

            if (idx < elements.Count - 1)
            {
                Reorder(el, idx + 1);
            }
            });
		}

		public void DeleteSelectedElements()
		{
            selectedAnchor = null;
            showingAnchorsElement = null;
            dragging = false;

            // TODO: Optimize for redrawing just selected elements (we remove call to DeleteElement when we do this)
            selectedElements.ForEach(el =>
            {
                el.GroupChildren.ForEach(child => child.Parent = null);
                DeleteElement(el);
            });

            selectedElements.Clear();
            canvas.Invalidate();
		}

        public void DeleteElement(GraphicElement el)
        {
            // TODO: don't redraw all the elements, only erase the current element and update the screen!
            // See how this is done with Ungroup.
            el.DetachAll();
            EraseTopToBottom(elements);
            elements.Remove(el);
            el.Dispose();
            DrawBottomToTop(elements);
        }

        protected void Reorder(GraphicElement el, int n)
		{
			EraseTopToBottom(elements);
			elements.Swap(n, elements.IndexOf(el));
			DrawBottomToTop(elements);
			UpdateScreen(elements);
		}

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
            var els = EraseTopToBottom(el, dx, dy);
			el.DisplayRectangle = newRect;
			el.UpdatePath();
			DrawBottomToTop(els, dx, dy);
			UpdateScreen(els, dx, dy);
		}

        public GroupBox GroupShapes(List<GraphicElement> shapesToGroup)
        {
            GroupBox groupBox = null;

            groupBox = new GroupBox(canvas);
            groupBox.GroupChildren.AddRange(shapesToGroup);
            Rectangle r = GetExtents(shapesToGroup);
            r.Inflate(5, 5);
            groupBox.DisplayRectangle = r;
            shapesToGroup.ForEach(s => s.Parent = groupBox);
            IEnumerable<GraphicElement> intersections = FindAllIntersections(groupBox);
            EraseTopToBottom(intersections);
            elements.Add(groupBox);

            intersections = FindAllIntersections(groupBox);
            DrawBottomToTop(intersections);
            UpdateScreen(intersections);

            return groupBox;
        }

        public void UngroupShapes(List<GraphicElement> shapesToUngroup)
        {
            List<GraphicElement> groupBoxesToRemove = new List<GraphicElement>();

            List<GraphicElement> intersections = new List<GraphicElement>();

            shapesToUngroup.ForEach(el =>
            {
                intersections.AddRange(FindAllIntersections(el));
            });

            // Preserve the original list, including the group boxes, for when we update the screen,
            // So that the erased groupbox region is updated on the screen.
            List<GraphicElement> originalIntersections = new List<GraphicElement>(intersections);

            foreach (GraphicElement el in shapesToUngroup)
            {
                if (el.GroupChildren.Any())
                {
                    groupBoxesToRemove.Add(el);
                    el.GroupChildren.ForEach(c => c.Parent = null);
                }
            }

            EraseTopToBottom(intersections.AsEnumerable());

            // Remove from elements collection and remove from intersections so only
            // the children are redrawn.
            groupBoxesToRemove.ForEach(gb =>
            {
                elements.Remove(gb);
                intersections.Remove(gb);
            });

            DrawBottomToTop(intersections.AsEnumerable());
            UpdateScreen(originalIntersections);        // remember, this updates the screen for the now erased groupbox.
        }

        public void MoveSelectedElements(Point delta)
        {
            int dx = delta.X.Abs();
            int dy = delta.Y.Abs();
            List<GraphicElement> intersections = new List<GraphicElement>();

            selectedElements.ForEach(el =>
            {
                intersections.AddRange(FindAllIntersections(el));
            });

            IEnumerable<GraphicElement> distinctIntersections = intersections.Distinct();
            List<GraphicElement> connectors = new List<GraphicElement>();

            selectedElements.ForEach(el =>
            {
                el.Connections.ForEach(c =>
                {
                    if (!connectors.Contains(c.ToElement))
                    {
                        connectors.Add(c.ToElement);
                    }
                });
            });

            EraseTopToBottom(distinctIntersections);

            connectors.ForEach(c =>
            {
                c.Move(delta);
                c.UpdatePath();
            });

            selectedElements.ForEach(el =>
            {
                // TODO: Kludgy workaround for dealing with multiple shape dragging with connectors in the selection list.
                if (!el.IsConnector)
                {
                    el.Move(delta);
                    el.UpdatePath();
                }
            });

            DrawBottomToTop(distinctIntersections, dx, dy);
            UpdateScreen(distinctIntersections, dx, dy);
        }

        public void MoveElement(GraphicElement el, Point delta)
		{
			if (el.OnScreen())
			{
                int dx = delta.X.Abs();
                int dy = delta.Y.Abs();
                var els = EraseTopToBottom(el, dx, dy);
				el.Move(delta);
				el.UpdatePath();
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

        // "Smart" move, erases everything first, moves all elements, then redraws them.
        public void MoveAllElements(Point delta)
        {
            EraseTopToBottom(elements);

            // Don't move grouped children, as the groupbox will do this for us when it moves.
            elements.Where(e=>e.Parent == null).ForEach(e =>
            {
                e.Move(delta);
                e.UpdatePath();
            });

            int dx = delta.X.Abs();
            int dy = delta.Y.Abs();
            DrawBottomToTop(elements, dx, dy);
            UpdateScreen(elements, dx, dy);
        }

        public void SaveAsPng(string filename)
		{
			// Get boundaries of of all elements.
			int x1 = elements.Min(e => e.DisplayRectangle.X);
			int y1 = elements.Min(e => e.DisplayRectangle.Y);
			int x2 = elements.Max(e => e.DisplayRectangle.X + e.DisplayRectangle.Width);
			int y2 = elements.Max(e => e.DisplayRectangle.Y + e.DisplayRectangle.Height);
			int w = x2 - x1 + 10;
			int h = y2 - y1 + 10;
			Canvas pngCanvas = new Canvas();                                      
			pngCanvas.CreateBitmap(w, h);
			Graphics gr = pngCanvas.AntiAliasGraphics;

			gr.Clear(Color.White);
			Point offset = new Point(-(x1-5), -(y1-5));
			Point restore = new Point(x1-5, y1-5);

			elements.AsEnumerable().Reverse().ForEach(e =>
			{
				e.Move(offset);
				e.UpdatePath();
				e.SetCanvas(pngCanvas);
				e.Draw(gr);
				e.DrawText(gr);
				e.SetCanvas(canvas);
				e.Move(restore);
				e.UpdatePath();
			});

			pngCanvas.Bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
			pngCanvas.Dispose();
		}

        public IEnumerable<GraphicElement> FindAllIntersections(GraphicElement el, int dx = 0, int dy = 0)
        {
            List<GraphicElement> intersections = new List<GraphicElement>();
            RecursiveFindAllIntersections(intersections, el, dx, dy);
            
            return intersections.OrderBy(e => elements.IndexOf(e));
        }

        protected Rectangle GetExtents(List<GraphicElement> elements)
        {
            Rectangle r = elements[0].DisplayRectangle;

            elements.Skip(1).ForEach(el =>
            {
                r = Rectangle.Union(r, el.DisplayRectangle);
            });

            return r;
        }

        protected void UpdateConnections(GraphicElement el)
        {
            el.Connections.ForEach(c =>
            {
                // Connection point on shape.
                var cps = el.GetConnectionPoints().Where(cp2 => cp2.Type == c.ElementConnectionPoint.Type);
                cps.ForEach(cp => c.ToElement.MoveAnchor(cp, c.ToConnectionPoint));
            });
        }

        /// <summary>
        /// Recursive loop to get all intersecting rectangles, including intersectors of the intersectees, so that all elements that
        /// are affected by an overlap redraw are erased and redrawn, otherwise we get artifacts of some intersecting elements when intersection count > 2.
        /// </summary>
        private void RecursiveFindAllIntersections(List<GraphicElement> intersections, GraphicElement el, int dx = 0, int dy = 0)
		{
			// Cool thing here is that if the element has no intersections, this list still returns that element because it intersects with itself!
			elements.Where(e => !intersections.Contains(e) && e.UpdateRectangle.IntersectsWith(el.UpdateRectangle.Grow(dx, dy))).ForEach((e) =>
			{
				intersections.Add(e);
                RecursiveFindAllIntersections(intersections, e);
			});
		}

		protected IEnumerable<GraphicElement> EraseTopToBottom(GraphicElement el, int dx = 0, int dy = 0)
		{
            Trace.WriteLine("Shape:EraseTopToBottom");
            IEnumerable<GraphicElement> intersections = FindAllIntersections(el, dx, dy);
			intersections.Where(e => e.OnScreen(dx, dy)).ForEach(e => e.Erase());

			return intersections;
		}

		public void EraseTopToBottom(IEnumerable<GraphicElement> els)
		{
            Trace.WriteLine("Shape:EraseTopToBottom");
			els.Where(e => e.OnScreen()).ForEach(e => e.Erase());
		}

		public void DrawBottomToTop(IEnumerable<GraphicElement> els, int dx = 0, int dy = 0)
		{
            Trace.WriteLine("Shape:DrawBottomToTop");
            els.Reverse().Where(e => e.OnScreen(dx, dy)).ForEach(e =>
			{
				e.GetBackground();
				e.Draw();
			});
		}

		public void UpdateScreen(IEnumerable<GraphicElement> els, int dx = 0, int dy = 0)
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
