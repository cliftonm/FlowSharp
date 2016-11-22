/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FlowSharpLib
{
    // Test Selected property to determine if the element is selected or de-selected.
	public class ElementEventArgs : EventArgs
	{
		public GraphicElement Element { get; set; }
	}

	public class SnapInfo
	{
		public GraphicElement NearElement { get; set; }
		public ConnectionPoint LineConnectionPoint { get; set; }
        public ConnectionPoint NearConnectionPoint { get; set; }
        public int AbsDx { get; set; }
        public int AbsDy { get; set; }
    }

	public class CanvasController : BaseController
	{
        // Minimum movement to start selection process.
        public const int SELECTION_MIN = 2;

		protected Point mousePosition;
        protected Point startSelectionPosition;
        protected Point currentSelectionPosition;
        protected GraphicElement selectionBox;

        public CanvasController(Canvas canvas) : base(canvas)
		{
			canvas.Controller = this;
			canvas.PaintComplete = CanvasPaintComplete;
		}

        public override void DragSelectedElements(Point delta)
        {
            MoveSelectedElements(delta);
        }

        public override void DeselectCurrentSelectedElements()
        {
            selectedElements.ForEach(el =>
            {
                var els = EraseIntersectionsTopToBottom(el);
                el.Deselect();
                DrawBottomToTop(els);
                UpdateScreen(els);
            });

            selectedElements.Clear();
            ElementSelected.Fire(this, new ElementEventArgs() { Element = null });
        }

        public override void DeselectGroupedElements()
        {
            List<GraphicElement> elementsToRemove = new List<GraphicElement>();

            selectedElements.Where(el=>el.Parent != null).ForEach(el =>
            {
                var els = EraseIntersectionsTopToBottom(el);
                el.Deselect();
                DrawBottomToTop(els);
                UpdateScreen(els);
                elementsToRemove.Add(el);
            });

            elementsToRemove.ForEach(el => selectedElements.Remove(el));

            if (selectedElements.Count == 0)
            {
                ElementSelected.Fire(this, new ElementEventArgs() { Element = null });
            }
            else
            {
                // Select the first element.
                // TODO: This needs to fire an event that can handle the group of selected elements,
                // particularly for PropertyGrid handling.
                ElementSelected.Fire(this, new ElementEventArgs() { Element = selectedElements[0] });
            }
        }

        public override void SelectElement(GraphicElement el)
        {
            // Add to selected elements only once!
            if (!selectedElements.Contains(el))
            {
                var els = EraseIntersectionsTopToBottom(el);
                selectedElements.Add(el);
                el.Select();
                DrawBottomToTop(els);
                UpdateScreen(els);
                ElementSelected.Fire(this, new ElementEventArgs() { Element = el });
            }
        }

        // Deselect all other elements.
        public override void SelectOnlyElement(GraphicElement el)
        {
            List<GraphicElement> intersections = FindAllIntersections(el).ToList();
            List<GraphicElement> deselectedElements = new List<GraphicElement>();
            selectedElements.Where(e => e != el).ForEach(e =>
            {
                intersections.AddRange(FindAllIntersections(e));
                e.Deselect();
                deselectedElements.Add(e);
            });

            if (!selectedElements.Contains(el))
            {
                selectedElements.Add(el);
                el.Select();
            }

            deselectedElements.ForEach(e => selectedElements.Remove(e));

            EraseTopToBottom(intersections);
            DrawBottomToTop(intersections);
            UpdateScreen(intersections);
            ElementSelected.Fire(this, new ElementEventArgs() { Element = el });
        }

        public override void DeselectElement(GraphicElement el)
        {
            IEnumerable<GraphicElement> intersections = FindAllIntersections(el);
            EraseTopToBottom(intersections);
            el.Deselect();
            selectedElements.Remove(el);
            DrawBottomToTop(intersections);
            UpdateScreen(intersections);
        }

        public override void SetAnchorCursor(GraphicElement el)
        {
            ShapeAnchor anchor = el.GetAnchors().FirstOrDefault(a => a.Near(mousePosition));
            canvas.Cursor = anchor == null ? Cursors.Arrow : anchor.Cursor;
        }

		protected bool SelectElement(Point p)
		{
			GraphicElement el = elements.FirstOrDefault(e => e.IsSelectable(p));

			if (el != null)
			{
				SelectElement(el);
			}

			return el != null;
		}
	}
}