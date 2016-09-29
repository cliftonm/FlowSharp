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
					// Snap the anchor?
					bool connectorAttached = selectedElement.SnapCheck(selectedAnchor, delta);

					if (!connectorAttached)
					{
						selectedElement.DisconnectShapeFromConnector(selectedAnchor.Type);
						selectedElement.RemoveConnection(selectedAnchor.Type);
					}
				}
				else
				{
					DragSelectedElement(delta);
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
				GraphicElement el = elements.FirstOrDefault(e => e.IsSelectable(mousePosition));

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

		public void DragSelectedElement(Point delta)
		{
			// We can snap a line if moving.
			// TODO: Moving a dynamic connector should snap as well, but the process has a bug - it snaps too soon and the line disappears!
			// Implementation in DynamicConnector is currently missing.
			bool connectorAttached = selectedElement.SnapCheck(GripType.Start, ref delta) || selectedElement.SnapCheck(GripType.End, ref delta);

			selectedElement.Connections.ForEach(c => c.ToElement.MoveElementOrAnchor(c.ToConnectionPoint.Type, delta));
			MoveElement(selectedElement, delta);
			UpdateSelectedElement.Fire(this, new ElementEventArgs() { Element = SelectedElement });

			if (!connectorAttached)
			{
				DetachFromAllShapes(selectedElement);
			}
		}

		protected void DetachFromAllShapes(GraphicElement el)
		{
			el.DisconnectShapeFromConnector(GripType.Start);
			el.DisconnectShapeFromConnector(GripType.End);
			el.RemoveConnection(GripType.Start);
			el.RemoveConnection(GripType.End);
		}

		public override bool Snap(GripType type, ref Point delta)
		{
			bool snapped = false;

			// Look for connection points on nearby elements.
			// If a connection point is nearby, and the delta is moving toward that connection point, then snap to that connection point.

			// So, it seems odd that we're using the connection points of the line, rather than the anchors.
			// However, this is actually simpler, and a line's connection points should at least include the endpoint anchors.
			IEnumerable<ConnectionPoint> connectionPoints = selectedElement.GetConnectionPoints().Where(p => type == GripType.None || p.Type == type);
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
							//si.NearElement.Connections.RemoveAll(c => c.ToElement == selectedElement);
							//selectedElement.RemoveConnection(si.LineConnectionPoint.Type);
							selectedElement.DisconnectShapeFromConnector(type);
							selectedElement.RemoveConnection(type);
						}
						else
						{
							// (A) If not already connected...
							if (!si.NearElement.Connections.Any(c => c.ToElement == selectedElement))
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

		protected virtual List<SnapInfo> GetNearbyElements(IEnumerable<ConnectionPoint> connectionPoints)
		{
			List<SnapInfo> nearElements = new List<SnapInfo>();

			elements.Where(e=>e != selectedElement && e.OnScreen() && (!(e is Line || e is DynamicConnector))).ForEach(e =>
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
				// e.HideConnectionPoints = !state;
				Redraw(e, CONNECTION_POINT_SIZE, CONNECTION_POINT_SIZE);
			});
		}

		public void DeselectCurrentSelectedElement()
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
			GraphicElement el = elements.FirstOrDefault(e => e.IsSelectable(p));

			if (el != null)
			{
				SelectElement(el);
			}

			return el != null;
		}

		public void SelectElement(GraphicElement el)
		{
			var els = EraseTopToBottom(el);
			el.Selected = true;
			DrawBottomToTop(els);
			UpdateScreen(els);
			selectedElement = el;
		}
	}
}