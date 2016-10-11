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
	}

	public class CanvasController : BaseController
	{
		protected Point mousePosition;
		protected List<SnapInfo> currentlyNear = new List<SnapInfo>();
		
		public CanvasController(Canvas canvas, List<GraphicElement> elements) : base(canvas, elements)
		{
			canvas.Controller = this;
			canvas.PaintComplete = CanvasPaintComplete;
            canvas.MouseDown += OnMouseDown;
            canvas.MouseUp += OnMouseUp;
			canvas.MouseMove += OnMouseMove;
		}

        public void DragSelectedElements(Point delta)
        {
            selectedElements.ForEach(el =>
            {
                bool connectorAttached = el.SnapCheck(GripType.Start, ref delta) || el.SnapCheck(GripType.End, ref delta);
                el.Connections.ForEach(c => c.ToElement.MoveElementOrAnchor(c.ToConnectionPoint.Type, delta));
                MoveElement(el, delta);
                UpdateSelectedElement.Fire(this, new ElementEventArgs() { Element = el });

                if (!connectorAttached)
                {
                    DetachFromAllShapes(el);
                }
            });
        }

        public void DeselectCurrentSelectedElements()
        {
            selectedElements.ForEach(el =>
            {
                var els = EraseTopToBottom(el);
                el.Selected = false;
                DrawBottomToTop(els);
                UpdateScreen(els);
            });

            selectedElements.Clear();
        }

        public void SelectElement(GraphicElement el)
        {
            DeselectCurrentSelectedElements();
            var els = EraseTopToBottom(el);
            selectedElements.Add(el);
            el.Selected = true;
            DrawBottomToTop(els);
            UpdateScreen(els);
            ElementSelected.Fire(this, new ElementEventArgs() { Element = el });
        }

        public override bool Snap(GripType type, ref Point delta)
        {
            // Snapping permitted only when one and only one element is selected.
            if (selectedElements.Count != 1) return false;

            bool snapped = false;
            GraphicElement selectedElement = selectedElements[0];

            // Look for connection points on nearby elements.
            // If a connection point is nearby, and the delta is moving toward that connection point, then snap to that connection point.

            // So, it seems odd that we're using the connection points of the line, rather than the anchors.
            // However, this is actually simpler, and a line's connection points should at least include the endpoint anchors.
            IEnumerable<ConnectionPoint> connectionPoints = selectedElement.GetConnectionPoints().Where(p => type == GripType.None || p.Type == type);
            List<SnapInfo> nearElements = GetNearbyElements(connectionPoints);
            ShowConnectionPoints(nearElements.Select(e => e.NearElement), true);
            ShowConnectionPoints(currentlyNear.Where(e => !nearElements.Any(e2 => e.NearElement == e2.NearElement)).Select(e => e.NearElement), false);
            currentlyNear = nearElements;

            foreach (SnapInfo si in nearElements)
            {
                ConnectionPoint nearConnectionPoint = si.NearElement.GetConnectionPoints().FirstOrDefault(cp => cp.Point.IsNear(si.LineConnectionPoint.Point, SNAP_CONNECTION_POINT_RANGE));

                if (nearConnectionPoint != null)
                {
                    Point sourceConnectionPoint = si.LineConnectionPoint.Point;
                    int neardx = nearConnectionPoint.Point.X - sourceConnectionPoint.X;     // calculate to match possible delta sign
                    int neardy = nearConnectionPoint.Point.Y - sourceConnectionPoint.Y;
                    int neardxsign = neardx.Sign();
                    int neardysign = neardy.Sign();
                    int deltaxsign = delta.X.Sign();
                    int deltaysign = delta.Y.Sign();

                    // Are we attached already or moving toward the shape's connection point?
                    if ((neardxsign == 0 || deltaxsign == 0 || neardxsign == deltaxsign) &&
                            (neardysign == 0 || deltaysign == 0 || neardysign == deltaysign))
                    {
                        // If attached, are we moving away from the connection point to detach it?
                        if (neardxsign == 0 && neardxsign == 0 && (delta.X.Abs() >= SNAP_DETACH_VELOCITY || delta.Y.Abs() >= SNAP_DETACH_VELOCITY))
                        {
                            selectedElement.DisconnectShapeFromConnector(type);
                            selectedElement.RemoveConnection(type);
                        }
                        else
                        {
                            // Not already connected?
                            // if (!si.NearElement.Connections.Any(c => c.ToElement == selectedElement))
                            if (neardxsign != 0 || neardysign != 0)
                            {
                                si.NearElement.Connections.Add(new Connection() { ToElement = selectedElement, ToConnectionPoint = si.LineConnectionPoint, ElementConnectionPoint = nearConnectionPoint });
                                selectedElement.SetConnection(si.LineConnectionPoint.Type, si.NearElement);
                            }

                            delta = new Point(neardx, neardy);
                            snapped = true;
                            break;
                        }
                    }
                }
            }

            return snapped;
        }

        public void StartDraggingMode(GraphicElement el, Point mousePos)
        {
            mousePosition = mousePos;
            leftMouseDown = true;
            dragging = true;

            //DeselectCurrentSelectedElements();
            //selectedElement = el;
            //selectedAnchor = selectedElement?.GetAnchors().FirstOrDefault(a => a.Near(mousePosition));
            //ElementSelected.Fire(this, new ElementEventArgs() { Element = selectedElement });
        }

        public void EndDraggingMode()
        {
            canvas.Cursor = Cursors.Arrow;
            selectedAnchor = null;
            leftMouseDown = false;
            dragging = false;
            ShowConnectionPoints(currentlyNear.Select(e => e.NearElement), false);
            currentlyNear.Clear();
        }

        public void DragShape(Point newMousePosition)
        { 
			Point delta = newMousePosition.Delta(mousePosition);

			// Weird - on click, the mouse move event appears to fire as well, so we need to check
			// for no movement in order to prevent detaching connectors!
			if (delta == Point.Empty) return;

			mousePosition = newMousePosition;

			if (dragging)
			{
				if (selectedAnchor != null)
				{
					// Snap the anchor?
                    // Only one element can be selected if moving an anchor.
					bool connectorAttached = selectedElements[0].SnapCheck(selectedAnchor, delta);

					if (!connectorAttached)
					{
						selectedElements[0].DisconnectShapeFromConnector(selectedAnchor.Type);
						selectedElements[0].RemoveConnection(selectedAnchor.Type);
					}
				}
				else
				{
					DragSelectedElements(delta);
				}

			}
			else if (leftMouseDown)
			{
                canvas.Cursor = Cursors.SizeAll;
                // Pick up every object on the canvas and move it.
                // This does not "move" the grid.
                MoveAllElements(delta);

				// Conversely, we redraw the grid and invalidate, which forces all the elements to redraw.
				//canvas.Drag(delta);
				//elements.ForEach(el => el.Move(delta));
				//canvas.Invalidate();
			}
			else
			{
				GraphicElement el = elements.FirstOrDefault(e => e.IsSelectable(mousePosition));

                // Remove anchors from current object being moused over and show, if an element selected on new object.
                if (el != showingAnchorsElement)
                {
                    if (showingAnchorsElement != null)
                    {
                        showingAnchorsElement.ShowAnchors = false;
                        Redraw(showingAnchorsElement);
                        showingAnchorsElement = null;
                        canvas.Cursor = Cursors.Arrow;
                    }

                    if (el != null)
                    {
                        el.ShowAnchors = true;
                        Redraw(el);
                        showingAnchorsElement = el;
                        SetAnchorCursor(el);
                    }
                }
                else if (el != null && el == showingAnchorsElement)
                {
                    // Same element is still selected, update cursor.
                    SetAnchorCursor(el);
                }
            }
		}

        protected void SetAnchorCursor(GraphicElement el)
        {
            ShapeAnchor anchor = el.GetAnchors().FirstOrDefault(a => a.Near(mousePosition));
            canvas.Cursor = anchor == null ? Cursors.Arrow : anchor.Cursor;
        }

        protected void OnMouseDown(object sender, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                leftMouseDown = true;
                DeselectCurrentSelectedElements();
                SelectElement(args.Location);
                selectedAnchor = null;
                
                if (selectedElements.Count == 1)
                {
                    selectedAnchor = selectedElements.Last()?.GetAnchors().FirstOrDefault(a => a.Near(mousePosition));
                    ElementSelected.Fire(this, new ElementEventArgs() { Element = selectedElements.Last() });
                }

                dragging = selectedElements.Any();
                mousePosition = args.Location;

                if ((selectedElements.Any()) && (selectedAnchor == null))
                {
                    canvas.Cursor = Cursors.SizeAll;
                }
                else if (selectedAnchor != null)
                {
                    SetAnchorCursor(selectedElements[0]);
                }
            }
        }

        protected void OnMouseUp(object sender, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                EndDraggingMode();
            }
        }

        protected void OnMouseMove(object sender, MouseEventArgs args)
        {
            DragShape(args.Location);
        }

        protected void DetachFromAllShapes(GraphicElement el)
		{
			el.DisconnectShapeFromConnector(GripType.Start);
			el.DisconnectShapeFromConnector(GripType.End);
			el.RemoveConnection(GripType.Start);
			el.RemoveConnection(GripType.End);
		}

		protected virtual List<SnapInfo> GetNearbyElements(IEnumerable<ConnectionPoint> connectionPoints)
		{
			List<SnapInfo> nearElements = new List<SnapInfo>();

			elements.Where(e=>e != selectedElements[0] && e.OnScreen() && !e.IsConnector).ForEach(e =>
			{
				Rectangle checkRange = e.DisplayRectangle.Grow(SNAP_ELEMENT_RANGE);

				connectionPoints.ForEach(cp =>
				{
					if (checkRange.Contains(cp.Point))
					{
						nearElements.Add(new SnapInfo() { NearElement = e, LineConnectionPoint = cp });
					}
				});
			});

			return nearElements;
		}

		protected virtual void ShowConnectionPoints(IEnumerable<GraphicElement> elements, bool state)
		{ 
			elements.ForEach(e =>
			{
				e.ShowConnectionPoints = state;
				Redraw(e, CONNECTION_POINT_SIZE, CONNECTION_POINT_SIZE);
			});
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