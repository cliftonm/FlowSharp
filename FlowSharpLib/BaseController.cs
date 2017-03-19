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
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;

namespace FlowSharpLib
{
    public class ZOrderMap
    {
        public GraphicElement Element { get; set; }
        public int Index { get; set; }
        public List<GraphicElement> GroupChildren { get; set; }
        public List<Connection> Connections { get; set; }

        // For connectors:
        public GraphicElement StartConnectedShape { get; set; }
        public GraphicElement EndConnectedShape { get; set; }
        public Connection StartConnection { get; set; }
        public Connection EndConnection { get; set; }
    }

    public abstract class BaseController
	{
        public event EventHandler<EventArgs> CanvasNameChanged;
        public event EventHandler<EventArgs> FilenameChanged;

        public const int MIN_WIDTH = 20;
        public const int MIN_HEIGHT = 20;

		public const int CONNECTION_POINT_SIZE = 3;		// this is actually the length from center.
        public const int GROUPBOX_INFLATE = 15;

        public const int CAP_WIDTH = 5;
        public const int CAP_HEIGHT = 5;

        public EventHandler<ElementEventArgs> ElementSelected;
        public EventHandler<ElementEventArgs> UpdateSelectedElement;

        public Canvas Canvas { get { return canvas; } }

        public string Filename
        {
            get { return filename; }
            set
            {
                filename = value;
                FilenameChanged.Fire(this, EventArgs.Empty);
            }
        }

        public string CanvasName
        {
            get { return canvasName; }
            set
            {
                canvasName = value;
                CanvasNameChanged.Fire(this, EventArgs.Empty);
            }
        }


        // TODO: Return back to ReadOnlyCollection and implement the functions that the menu controller needs.
        public ReadOnlyCollection<GraphicElement> Elements { get { return elements.AsReadOnly(); } }
        
        // TODO: Implement as interface
        // public MouseController MouseController { get; set; }

        // TODO: Kludgy workaround for issue #34.
        public bool IsCanvasDragging { get; set; }

        // Ignore snap if Ctrl key is pressed when "doing".
        // Undo/redo is more complicated, especially since it can be activated with a keyboard Ctrl+Z or Ctrl+Y
        // which means the ctrl key is pressed.  Furthermore, the original ignore snap needs to be preserved.
        public bool UndoRedoIgnoreSnapCheck { get; set; }
        public bool IsSnapToBeIgnored { get { return ((Control.ModifierKeys & Keys.Control) == Keys.Control) || UndoRedoIgnoreSnapCheck; } }

        public UndoStack UndoStack { get { return undoStack; } }
        public ReadOnlyCollection<GraphicElement> SelectedElements { get { return selectedElements.AsReadOnly(); } }

        public SnapController SnapController { get; protected set; }

        public int Zoom { get; protected set; }

        protected List<GraphicElement> elements;
		protected Canvas canvas;
        protected UndoStack undoStack;
		protected List<GraphicElement> selectedElements;
        protected string canvasName;
        protected string filename;

        // Diagnostic
        protected int eraseCount = 0;

		public BaseController(Canvas canvas)
		{
            undoStack = new UndoStack();
			this.canvas = canvas;
            elements = new List<GraphicElement>();
            selectedElements = new List<GraphicElement>();
            SnapController = new SnapController(this);
            Zoom = 100;
		}

        public virtual bool IsMultiSelect()
        {
            return !((Control.ModifierKeys & (Keys.Control | Keys.Shift)) == 0);
        }

        // TODO: These empty base class methods are indicative of bad design.
        public virtual void SelectElement(GraphicElement el) { }
        public virtual void SelectOnlyElement(GraphicElement el) { }
        public virtual void SetAnchorCursor(GraphicElement el) { }
        public virtual void DragSelectedElements(Point delta) { }
        public virtual void DeselectCurrentSelectedElements() { }
        public virtual void DeselectGroupedElements() { }
        public virtual void DeselectElement(GraphicElement el) { }

        public void Insert(int idx, GraphicElement el)
        {
            elements.Insert(idx, el);
        }

        public void AddElement(GraphicElement el)
        {
            elements.Add(el);
        }

        public void AddElements(List<GraphicElement> els)
        {
            elements.AddRange(els);
        }

        public void SaveChildZOrder(GraphicElement el, List<ZOrderMap> zorder)
        {
            el.GroupChildren.ForEach(gc =>
            {
                ZOrderMap zom = new ZOrderMap() { Element = gc, Index = elements.IndexOf(gc) };
                zom.GroupChildren = new List<GraphicElement>(gc.GroupChildren);
                zom.Connections = new List<Connection>(gc.Connections);
                zorder.Add(zom);
                SaveChildZOrder(gc, zorder);
            });
        }

        // TODO: This does more than just getting the zorder - it also saves connection information for connectors, 
        // which is critical to wire up connectors to shapes after a delete has been undone.
        public List<ZOrderMap> GetZOrderOfSelectedElements()
        {
            List<ZOrderMap> originalZOrder = new List<ZOrderMap>();

            selectedElements.ForEach(el =>
            {
                ZOrderMap zom = new ZOrderMap() { Element = el, Index = elements.IndexOf(el) };
                zom.GroupChildren = new List<GraphicElement>(el.GroupChildren);
                zom.Connections = new List<Connection>(el.Connections);

                if (el.IsConnector)
                {
                    zom.StartConnectedShape = ((Connector)el).StartConnectedShape;
                    zom.EndConnectedShape = ((Connector)el).EndConnectedShape;

                    if (zom.StartConnectedShape != null)
                    {
                        // TODO: First or default used because we have a bug, yet to fix, where the shape can have the same connector attached twice!
                        zom.StartConnection = zom.StartConnectedShape.Connections.FirstOrDefault(conn => conn.ToElement == el);
                    }

                    if (zom.EndConnectedShape != null)
                    {
                        // TODO: First or default used because we have a bug, yet to fix, where the shape can have the same connector attached twice!
                        zom.EndConnection = zom.EndConnectedShape.Connections.FirstOrDefault(conn => conn.ToElement == el);
                    }
                }

                originalZOrder.Add(zom);
                SaveChildZOrder(el, originalZOrder);
            });

            return originalZOrder;
        }

        public void Clear()
        {
            elements.ForEach(el => el.Dispose());
            elements.Clear();
            selectedElements.Clear();
        }

        public virtual void Undo()
        {
            undoStack.Undo();
        }

        public virtual void Redo()
        {
            undoStack.Redo();
        }

        public bool IsRootShapeSelectable(Point p)
        {
            return elements.Any(e => e.IsSelectable(p) && e.Parent == null && e.Visible);
        }

        public bool IsChildShapeSelectable(Point p)
        {
            return elements.Any(e => e.IsSelectable(p) && e.Parent != null && e.Visible);
        }

        public GraphicElement GetRootShapeAt(Point p)
        {
            return elements.FirstOrDefault(e => e.IsSelectable(p) && e.Parent == null && e.Visible);
        }

        public GraphicElement GetChildShapeAt(Point p)
        {
            return elements.FirstOrDefault(e => e.IsSelectable(p) && e.Parent != null && e.Visible);
        }

        public void SelectElements(List<GraphicElement> els)
        {
            els.ForEach(el => SelectElement(el));
        }

        // Called when undo'ing a zorder move.
        public void RestoreZOrder(List<ZOrderMap> zorder)
        {
            // Remove all shapes from the elements list.
            zorder.Select(zo => zo.Element).ForEach(el => elements.Remove(el));
            // Insert them into the list in ascending order, so each insertion goes in the right place.
            zorder.OrderBy(zo => zo.Index).ForEach(zo => elements.Insert(zo.Index, zo.Element));
            // TODO: Redraw everything, because I'm lazy and because this actually might be the best way of getting all the pieces to play nice together.
            canvas.Invalidate();
        }

        // Called when undo'ing a UI initiated delete of selected shapes.
        public void RestoreZOrderWithHierarchy(List<ZOrderMap> zorder)
        {
            // Insert them into the list in ascending order, so each insertion goes in the right place.
            zorder.OrderBy(zo => zo.Index).ForEach(zo => elements.Insert(zo.Index, zo.Element));
            zorder.ForEach(zo => zo.Element.Connections = new List<Connection>(zo.Connections));

            zorder.ForEach(zo =>
            {
                zo.Element.GroupChildren = new List<GraphicElement>(zo.GroupChildren);
                zo.Element.GroupChildren.ForEach(gc => gc.Parent = zo.Element);
            });

            zorder.ForEach(zo => zo.Element.Restored());

            canvas.Invalidate();
        }

        public void Topmost()
		{
            // TODO: Sub-optimal, as we're erasing all elements.
            EraseTopToBottom(elements);

            // In their original z-order, but reversed because we're inserting at the top...
            selectedElements.OrderByDescending(el => elements.IndexOf(el)).ForEach(el =>
            {
                elements.Remove(el);
                elements.Insert(0, el);
                // Preserve child order.
                el.GroupChildren.OrderByDescending(child=>elements.IndexOf(child)).ForEach(child => MoveToTop(child));
            });

            DrawBottomToTop(elements);
            UpdateScreen(elements);
        }

        public void Bottommost()
		{
            // TODO: Sub-optimal, as we're erasing all elements.
            EraseTopToBottom(elements);

            // In their original z-oder, since we're appending to the bottom...
            selectedElements.OrderBy(el => elements.IndexOf(el)).ForEach(el =>
            {
                elements.Remove(el);
                // Preserve child order.
                el.GroupChildren.OrderBy(child=>elements.IndexOf(child)).ForEach(child => MoveToBottom(child));
                elements.Add(el);
            });

            DrawBottomToTop(elements);
            UpdateScreen(elements);
        }

        public void MoveSelectedElementsUp()
		{
            // TODO: Sub-optimal, as we're erasing all elements.
            EraseTopToBottom(elements);
            MoveUp(selectedElements);
            DrawBottomToTop(elements);
            UpdateScreen(elements);
        }

        public void MoveSelectedElementsDown()
        {
            // TODO: Sub-optimal, as we're erasing all elements.
            EraseTopToBottom(elements);
            MoveDown(selectedElements);
            DrawBottomToTop(elements);
            UpdateScreen(elements);
        }

        // Used by UI "delete shape", this is a recursive destruction of shapes and, if they are groupboxes, their child shapes, etc.
        public void DeleteSelectedElementsHierarchy(bool dispose = true)
		{
            // TODO: Optimize for redrawing just selected elements (we remove call to DeleteElement when we do this)
            selectedElements.ForEach(el =>
            {
                DeleteElementHierarchy(el, dispose);
                el.DetachAll();
                el.Connections.ForEach(c => c.ToElement.RemoveConnection(c.ToConnectionPoint.Type));
                el.Connections.Clear();
                RemoveElement(el, dispose);
            });

            selectedElements.Clear();
            canvas.Invalidate();
		}

        // Used by secondary operations, particularly undo events, where we delete things we've pasted or dropped onto the canvas.
        public void DeleteElement(GraphicElement el, bool dispose = true)
        {
            selectedElements.Remove(el);
            el.DetachAll();
            var els = EraseIntersectionsTopToBottom(el);
            List<GraphicElement> elsToRedraw = els.ToList();
            elsToRedraw.Remove(el);
            el.Connections.ForEach(c => c.ToElement.RemoveConnection(c.ToConnectionPoint.Type));
            el.Connections.Clear();
            DrawBottomToTop(elsToRedraw);
            UpdateScreen(els);          // Original list, so element that is being deleted is included in the region to update.
            RemoveElement(el, dispose);
        }

        public void Redraw(GraphicElement el, int dx=0, int dy=0)
		{
            Trace.WriteLine("*** Redraw1 " + el.GetType().Name);
			var els = EraseIntersectionsTopToBottom(el, dx, dy);
			DrawBottomToTop(els, dx, dy);
			UpdateScreen(els, dx, dy);
		}

		public void Redraw(GraphicElement el, Action<GraphicElement> afterErase)
		{
            Trace.WriteLine("*** Redraw2 " + el.GetType().Name);
            var els = EraseIntersectionsTopToBottom(el);
			UpdateScreen(els);
			afterErase(el);
            el.UpdatePath();
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
            UpdateSelectedElement.Fire(this, new ElementEventArgs() { Element = el });
        }

        /// <summary>
        /// Direct update of display rectangle, used in DynamicConnector.
        /// </summary>
        public void UpdateDisplayRectangle(GraphicElement el, Rectangle newRect, Point delta)
		{
            Trace.WriteLine("*** UpdateDisplayRectangle " + el.GetType().Name);
            int dx = delta.X.Abs();
			int dy = delta.Y.Abs();
            var els = EraseIntersectionsTopToBottom(el, dx, dy);
            el.DisplayRectangle = newRect;
			el.UpdatePath();
			DrawBottomToTop(els, dx, dy);
			UpdateScreen(els, dx, dy);
		}

        public GroupBox GroupShapes(GroupBox groupBox)
        {
            List<GraphicElement> shapesToGroup = selectedElements;
            groupBox.GroupChildren.AddRange(shapesToGroup);
            Rectangle r = GetExtents(shapesToGroup);
            r.Inflate(GROUPBOX_INFLATE, GROUPBOX_INFLATE);
            groupBox.DisplayRectangle = r;
            shapesToGroup.ForEach(s => s.Parent = groupBox);
            IEnumerable<GraphicElement> intersections = FindAllIntersections(groupBox);
            EraseTopToBottom(intersections);

            // Insert groupbox just after the lowest shape being grouped.
            int insertionPoint = shapesToGroup.Select(s => elements.IndexOf(s)).OrderBy(n => n).Last() + 1;
            elements.Insert(insertionPoint, groupBox);

            intersections = FindAllIntersections(groupBox);
            DrawBottomToTop(intersections);
            UpdateScreen(intersections);

            return groupBox;
        }

        public void UngroupShapes(GroupBox el, bool dispose=true)
        {
            List<GraphicElement> intersections = FindAllIntersections(el).ToList();

            // Preserve the original list, including the group boxes, for when we update the screen,
            // So that the erased groupbox region is updated on the screen.
            List<GraphicElement> originalIntersections = new List<GraphicElement>(intersections);

            el.GroupChildren.ForEach(c => c.Parent = null);
            el.GroupChildren.Clear();
            EraseTopToBottom(intersections.AsEnumerable());

            RemoveElement(el, dispose);
            intersections.Remove(el);

            DrawBottomToTop(intersections.AsEnumerable());
            UpdateScreen(originalIntersections);        // remember, this updates the screen for the now erased groupbox.
        }

        public void MoveSelectedElements(Point delta)
        {
            // TODO: We shouldn't even be calling this method if there are no selected elements!
            if (selectedElements.Count == 0) return;

            int dx = delta.X.Abs();
            int dy = delta.Y.Abs();
            List<GraphicElement> intersections = new List<GraphicElement>();

            selectedElements.ForEach(el =>
            {
                intersections.AddRange(FindAllIntersections(el));
            });

            IEnumerable<GraphicElement> distinctIntersections = intersections.Distinct();
            List<GraphicElement> connectors = new List<GraphicElement>();

            //selectedElements.ForEach(el =>
            //{
            //    el.Connections.ForEach(c =>
            //    {
            //        if (!connectors.Contains(c.ToElement))
            //        {
            //            connectors.Add(c.ToElement);
            //        }
            //    });
            //});

            EraseTopToBottom(distinctIntersections);

            //connectors.ForEach(c =>
            //{
            //    // X1
            //    //c.MoveUndoRedo(delta, false);
            //    c.Move(delta);
            //    c.UpdatePath();
            //});

            selectedElements.ForEach(el =>
            {
                // TODO: Kludgy workaround for dealing with multiple shape dragging with connectors in the selection list.
                // if (!el.IsConnector)
                {
                    // X1
                    //el.MoveUndoRedo(delta, false);
                    el.Move(delta);
                    el.UpdatePath();

                    // Issue #49 - multiple selected shapes don't move anchors/lines of connectors connectors/lines.
                    el.Connections.ForEach(c =>
                    {
                        if (!selectedElements.Contains(c.ToElement))
                        {
                            MoveLineOrAnchor(c, delta);
                        }
                    });


                }
            });

            DrawBottomToTop(distinctIntersections, dx, dy);
            UpdateScreen(distinctIntersections, dx, dy);
            UpdateSelectedElement.Fire(this, new ElementEventArgs() { Element = selectedElements[0] });
        }

        public void MoveLineOrAnchor(Connection c, Point delta)
        {
            // TODO: Improve this code, somehow.
            if (c.ToElement is Line)
            {
                c.ToElement.Move(delta);
            }
            else
            {
                c.ToElement.MoveAnchor(c.ToConnectionPoint.Type, delta);
            }
        }

        public void MoveElement(GraphicElement el, Point delta)
		{
			if (el.OnScreen())
			{
                Trace.WriteLine("*** MoveElement " + el.GetType().Name);
                int dx = delta.X.Abs();
                int dy = delta.Y.Abs();
                var els = EraseIntersectionsTopToBottom(el, dx, dy);
                // X1
                //el.MoveUndoRedo(delta, false);
                el.Move(delta);
				el.UpdatePath();
                UpdateConnections(el);
				DrawBottomToTop(els, dx, dy);
				UpdateScreen(els, dx, dy);
			}
			else
			{
				el.CancelBackground();
                // X1
                //el.MoveUndoRedo(delta, false);
                el.Move(delta);
				// TODO: Display element if moved back on screen at this point?
			}
		}

        // For canvas dragging.
        public void MoveAllElements(Point delta)
        {
            if (IsCanvasDragging)
            {
                Trace.WriteLine("*** MoveAllElements Collision");
            }

            IsCanvasDragging = true;            // Kludgy workaround for Issue #34 (groupbox update)
            EraseTopToBottom(elements);

            // Don't move grouped children, as the groupbox will do this for us when it moves.
            elements.Where(e => e.Parent == null).ForEach(e =>
                {
                    e.Move(delta);
                    e.UpdatePath();
                });

            int dx = delta.X.Abs();
            int dy = delta.Y.Abs();
            DrawBottomToTop(elements, dx, dy);
            UpdateScreen(elements, dx, dy);
            IsCanvasDragging = false;
        }

        public void SetZoom(int zoom)
        {
            // erase and ppdate the screen, removing elements after erasure, so they don't leave their images when the zoom factor is changed.
            EraseTopToBottom(elements);
            UpdateScreen(elements);         

            Zoom = zoom;
            elements.Where(e => e.Parent == null).ForEach(e => e.UpdatePath());
            DrawBottomToTop(elements);
            UpdateScreen(elements);
        }

        public void RedrawAllElements()
        {
            EraseTopToBottom(elements);

            // Don't move grouped children, as the groupbox will do this for us when it moves.
            elements.Where(e => e.Parent == null).ForEach(e => e.UpdatePath());
            DrawBottomToTop(elements);
            UpdateScreen(elements);
        }

        public void SaveAsPng(string filename, bool selectionOnly = false)
        {
            selectionOnly.If(() => SaveAsPng(filename, SelectedElements.ToList())).Else(() => SaveAsPng(filename, elements));
        }

        protected void SaveAsPng(string filename, List<GraphicElement> elements)
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
				e.Draw(gr, false);      // Don't draw selection or tag shapes.
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

        public void EraseTopToBottom(IEnumerable<GraphicElement> els)
        {
            if (++eraseCount > 1)
            {
                Trace.WriteLine("*** TOO MANY ERASE " + eraseCount + " ***");
            }

            Trace.WriteLine("Shape:EraseTopToBottom " + eraseCount);
            els.Where(e => e.OnScreen()).ForEach(e => e.Erase());
        }

        public void DrawBottomToTop(IEnumerable<GraphicElement> els, int dx = 0, int dy = 0)
        {
            if (--eraseCount < 0)
            {
                Trace.WriteLine("*** TOO MANY DRAW " + eraseCount + " ***");
            }

            Trace.WriteLine("Shape:DrawBottomToTop " + eraseCount);
            els.AsEnumerable().Reverse().Where(e => e.OnScreen(dx, dy)).ForEach(e =>
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

        /// <summary>
        /// Center the canvas on the selected element.
        /// This does not add the action to the undo stack.
        /// </summary>
        public void FocusOn(GraphicElement el)
        {
            int cx = (Canvas.Width - el.DisplayRectangle.Width) / 2;
            int cy = (Canvas.Height - el.DisplayRectangle.Height) / 2;
            int dx = -(el.DisplayRectangle.X - cx);
            int dy = -(el.DisplayRectangle.Y - cy);
            MoveAllElements(new Point(dx, dy));
        }

        public void ClearBookmarks()
        {
            Elements.ForEach(el =>
            {
                el.ClearBookmark();
                Redraw(el);
            });
        }

        protected virtual void RemoveElement(GraphicElement el, bool dispose)
        {
            elements.Remove(el);
            el.Removed(dispose);

            if (dispose)
            {
                el.Dispose();
            }
        }

        protected void DeleteElementHierarchy(GraphicElement el, bool dispose)
        {
            el.GroupChildren.ForEach(gc =>
            {
                DeleteElementHierarchy(gc, dispose);
                gc.DetachAll();
                gc.Connections.ForEach(c => c.ToElement.RemoveConnection(c.ToConnectionPoint.Type));
                gc.Connections.Clear();
                RemoveElement(gc, dispose);
            });
        }

        protected void MoveToTop(GraphicElement el)
        {
            elements.Remove(el);
            elements.Insert(0, el);
            el.GroupChildren.ForEach(child => MoveToTop(child));
        }

        protected void MoveToBottom(GraphicElement el)
        {
            elements.Remove(el);
            el.GroupChildren.ForEach(child => MoveToBottom(child));
            elements.Add(el);
        }

        // The reason for the complexity here in MoveUp/MoveDown is because we're not actually "containing" child elements
        // of a group box in a sub-list.  All child elements are actually part of the master, flat, z-ordered list of shapes (elements.)
        // This means we have to go through some interested machinations to properly move nested groupboxes, however the interesting
        // side effect to this is that, a non-grouped shape, can slide between shapes in a groupbox!

        protected void MoveUp(IEnumerable<GraphicElement> els)
        {
            // Since we're swapping up, order by z-order so we're always swapping with the element above,
            // thus preserving z-order of the selected shapes.

            // (from el in els select new { El = el, Idx = elements.IndexOf(el) }).OrderBy(item => item.Idx).ForEach(item =>
            els.OrderBy(el=>elements.IndexOf(el)).ForEach(el=>
            {
                // To handle groupboxes:
                // 1. Recursively get the list of all grouped shapes, which including sub-groups
                List<GraphicElement> childElements = new List<GraphicElement>();
                RecursiveGetAllGroupedShapes(el.GroupChildren, childElements);
                childElements = childElements.OrderBy(e => elements.IndexOf(e)).ToList();

                // 2. Delete all those elements, so we are working with root level shapes only.
                childElements.ForEach(child => elements.Remove(child));
                
                // 3. Now see if there's something to do.
                int idx = elements.IndexOf(el);
                int targetIdx = idx > 0 ? idx - 1 : idx;

                if (targetIdx != idx)
                {
                    elements.Swap(idx, idx - 1);
                }

                // 4. Insert the child elements above the element we just moved up, in reverse order.
                childElements.AsEnumerable().Reverse().ForEach(child => elements.Insert(targetIdx, child));
            });
        }

        protected void MoveDown(IEnumerable<GraphicElement> els)
        {
            // Since we're swapping down, order by z-oder descending so we're always swapping with the element below,
            // thus preserving z-order of the selected shapes.
            els.OrderByDescending(e => elements.IndexOf(e)).ForEach(el =>
            {
                // To handle groupboxes:
                // 1. Recursively get the list of all grouped shapes, which including sub-groups
                List<GraphicElement> childElements = new List<GraphicElement>();
                RecursiveGetAllGroupedShapes(el.GroupChildren, childElements);
                childElements = childElements.OrderBy(e => elements.IndexOf(e)).ToList();

                // 2. Delete all those elements, so we are working with root level shapes only.
                childElements.ForEach(child => elements.Remove(child));

                // 3. Now see if there's something to do.
                int idx = elements.IndexOf(el);
                int targetIdx = idx < elements.Count - 1 ? idx + 1 : idx;

                if (targetIdx != idx)
                {
                    elements.Swap(idx, idx + 1);
                }

                // 4. Insert the child elements above the element we just moved down, in reverse order.
                childElements.AsEnumerable().Reverse().ForEach(child => elements.Insert(targetIdx, child));
            });
        }

        protected void RecursiveGetAllGroupedShapes(List<GraphicElement> children, List<GraphicElement> acc)
        {
            acc.AddRange(children);
            children.ForEach(child => RecursiveGetAllGroupedShapes(child.GroupChildren, acc));
        }

        public Rectangle GetExtents(IEnumerable<GraphicElement> elements)
        {
            Rectangle r = elements.First().DisplayRectangle;
            elements.Skip(1).ForEach(el => r = r.Union(el.DisplayRectangle));

            return r;
        }

        public void UpdateConnections(GraphicElement el)
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
            int elIdx = elements.IndexOf(el);

			// Cool thing here is that if the element has no intersections, this list still returns that element because it intersects with itself!
            // Optimization here is that we only collect shapes that intersect and are above (on top of) the current shape. 
            // This optimization works really well!
			elements.Where(e => !intersections.Contains(e) && elements.IndexOf(e) <= elIdx && e.UpdateRectangle.IntersectsWith(el.UpdateRectangle.Grow(dx, dy))).ForEach((e) =>
			{
                intersections.Add(e);
                RecursiveFindAllIntersections(intersections, e);
			});
		}

		protected IEnumerable<GraphicElement> EraseIntersectionsTopToBottom(GraphicElement el, int dx = 0, int dy = 0)
		{
            if (++eraseCount > 1)
            {
                Trace.WriteLine("Shape:*** TOO MANY ERASE " + eraseCount + " ***");
            }

            Trace.WriteLine("Shape:EraseIntersectionsTopToBottom " + eraseCount);
            IEnumerable<GraphicElement> intersections = FindAllIntersections(el, dx, dy);
			intersections.Where(e => e.OnScreen(dx, dy)).ForEach(e => e.Erase());

			return intersections;
		}

		protected void CanvasPaintComplete(Canvas canvas)
		{
            Trace.WriteLine("*** CanvasPaintComplete");
            eraseCount = 1;         // Diagnostics
            DrawBottomToTop(elements);
		}
    }
}
