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

	public class SnapInfo
	{
		public GraphicElement NearElement { get; set; }
		public ConnectionPoint LineConnectionPoint { get; set; }
	}

	public class CanvasController : BaseController
	{
		public EventHandler<ElementEventArgs> ElementSelected;
		public EventHandler<ElementEventArgs> UpdateSelectedElement;

		public GraphicElement SelectedElement { get { return selectedElement; } }

		protected bool dragging;
		protected bool leftMouseDown;
		protected GraphicElement selectedElement;
		protected ShapeAnchor selectedAnchor;
		protected GraphicElement showingAnchorsElement;
		protected Point mousePosition;
		protected List<SnapInfo> currentlyNear = new List<SnapInfo>();
		
		public CanvasController(Canvas canvas, List<GraphicElement> elements) : base(canvas, elements)
		{
			canvas.Controller = this;
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
					ShowConnectionPoints(currentlyNear.Select(e => e.NearElement), false);
					currentlyNear.Clear();
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
					if (selectedElement is DynamicConnector)
					{
						// We can snap an endpoint
						//if (Snap(ref delta))
						//{
						//	selectedElement.Move(delta);
						//}
						//else
						{
						}
					}

					selectedElement.UpdateSize(selectedAnchor, delta);
					UpdateSelectedElement.Fire(this, new ElementEventArgs() { Element = SelectedElement });
				}
				else
				{
					// We can snap a line if moving.
					if (selectedElement is ILine)
					{
						Snap(ref delta);
					}

					MoveElement(selectedElement, delta);
					// TODO: For connections terminating on another element, move only the connection point, not the whole line.
					selectedElement.Connections.ForEach(c => MoveElement(c.ToElement, delta));
					UpdateSelectedElement.Fire(this, new ElementEventArgs() { Element = SelectedElement });
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
				GraphicElement el = elements.FirstOrDefault(e => e.UpdateRectangle.Contains(args.Location));

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

		protected virtual bool Snap(ref Point delta)
		{
			if (delta.X == 0 && delta.Y == 0) return false;
			bool snapped = false;

			// Look for connection points on nearby elements.
			// If a connection point is nearby, and the delta is moving toward that connection point, then snap to that connection point.

			List<ConnectionPoint> connectionPoints = selectedElement.GetConnectionPoints();
			List<SnapInfo> nearElements = GetNearbyElements(connectionPoints);
			ShowConnectionPoints(nearElements.Select(e=>e.NearElement), true);
			ShowConnectionPoints(currentlyNear.Where(e => !nearElements.Any(e2 => e.NearElement == e2.NearElement)).Select(e=>e.NearElement), false);
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

					if ((neardxsign == 0 || deltaxsign == 0 || neardxsign == deltaxsign) &&
							(neardysign == 0 || deltaysign == 0 || neardysign == deltaysign))
					{
						// Possible detach?
						if (neardxsign == 0 && neardxsign == 0 && (delta.X.Abs() >= SNAP_DETACH_VELOCITY || delta.Y.Abs() >= SNAP_DETACH_VELOCITY))
						{
							// TODO: Bug if both endpoints of the line are connected to the same selected element.  See (A) below.
							si.NearElement.Connections.RemoveAll(c => c.ToElement == selectedElement);
						}
						else
						{
							// (A) If not already connected...
							if (!si.NearElement.Connections.Any(c => c.ToElement == selectedElement))
							{
								si.NearElement.Connections.Add(new Connection() { ToElement = selectedElement, ToConnectionPoint = si.LineConnectionPoint, ElementConnectionPoint = nearConnectionPoint });
							}

							delta = new Point(neardx, neardy);
							snapped = true;
						}
					}
				}
			}

			return snapped;
		}

		protected virtual List<SnapInfo> GetNearbyElements(List<ConnectionPoint> connectionPoints)
		{
			List<SnapInfo> nearElements = new List<SnapInfo>();

			elements.Where(e=>e != selectedElement && e.OnScreen()).ForEach(e =>
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
				e.HideConnectionPoints = !state;
				Redraw(e, CONNECTION_POINT_SIZE, CONNECTION_POINT_SIZE);
			});
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
			GraphicElement el = elements.FirstOrDefault(e => e.UpdateRectangle.Contains(p));

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