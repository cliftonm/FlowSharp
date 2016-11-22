using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FlowSharpLib
{
    public class SnapAction
    {
        public enum Action
        {
            Attached,
            Attach,
            Detach,
        }

        public Point Delta { get; protected set; }
        public Action SnapType { get; protected set; }

        public GraphicElement Connector { get { return connector; } }
        public GraphicElement TargetShape { get { return targetShape; } }
        public GripType GripType { get { return gripType; } }
        public ConnectionPoint ShapeConnectionPoint { get { return shapeConnectionPoint; } }

        protected GraphicElement connector;
        protected GraphicElement targetShape;
        protected GripType gripType;
        protected ConnectionPoint lineConnectionPoint;
        protected ConnectionPoint shapeConnectionPoint;

        /// <summary>
        /// Used for specifying ignore mouse move, for when connector is attached but velocity is not sufficient to detach.
        /// </summary>
        /// <param name="action"></param>
        public SnapAction()
        {
            SnapType = Action.Attached;
        }

        public SnapAction(Action action, GraphicElement lineShape, GripType gripType, GraphicElement targetShape, ConnectionPoint lineConnectionPoint, ConnectionPoint shapeConnectionPoint, Point delta)
        {
            SnapType = action;
            this.connector = lineShape;
            this.gripType = gripType;
            this.targetShape = targetShape;
            this.lineConnectionPoint = lineConnectionPoint;
            this.shapeConnectionPoint = shapeConnectionPoint;
            Delta = delta;
        }

        public void Attach()
        {
            // action = new SnapAction(selectedElement, type, si.NearElement, si.LineConnectionPoint, nearConnectionPoint);
            //si.NearElement.Connections.Add(new Connection() { ToElement = selectedElement, ToConnectionPoint = si.LineConnectionPoint, ElementConnectionPoint = nearConnectionPoint });
            //selectedElement.SetConnection(si.LineConnectionPoint.Type, si.NearElement);
            targetShape.Connections.Add(new Connection() { ToElement = connector, ToConnectionPoint = lineConnectionPoint, ElementConnectionPoint = shapeConnectionPoint });
            connector.SetConnection(lineConnectionPoint.Type, targetShape);
        }

        public void Detach()
        {
            connector.DisconnectShapeFromConnector(gripType);
            connector.RemoveConnection(gripType);
        }

        public SnapAction Clone()
        {
            SnapAction ret = new SnapAction();
            ret.SnapType = SnapType;
            ret.connector = connector;
            ret.gripType = gripType;
            ret.targetShape = targetShape;
            ret.lineConnectionPoint = lineConnectionPoint;
            ret.shapeConnectionPoint = shapeConnectionPoint;
            ret.Delta = Delta;

            return ret;
        }
    }

    public class SnapController
    {
        protected BaseController controller;

        public SnapController(BaseController controller)
        {
            this.controller = controller;
        }

        public const int SNAP_ELEMENT_RANGE = 20;
        public const int SNAP_CONNECTION_POINT_RANGE = 10;
        public const int SNAP_DETACH_VELOCITY = 5;
        public const int CONNECTION_POINT_SIZE = 3;

        // ============== SNAP ==============
        // TODO: This could actually be in a separate controller?
        protected List<SnapInfo> currentlyNear = new List<SnapInfo>();
        protected List<SnapInfo> nearElements = new List<SnapInfo>();

        public SnapAction Snap(GripType type, Point delta, bool isByKeyPress = false)
        {
            SnapAction action = null;
            // Snapping permitted only when one and only one element is selected.
            if (controller.SelectedElements.Count != 1) return null;

            // bool snapped = false;
            GraphicElement selectedElement = controller.SelectedElements[0];

            // Look for connection points on nearby elements.
            // If a connection point is nearby, and the delta is moving toward that connection point, then snap to that connection point.

            // So, it seems odd that we're using the connection points of the line, rather than the anchors.
            // However, this is actually simpler, and a line's connection points should at least include the endpoint anchors.
            IEnumerable<ConnectionPoint> connectionPoints = selectedElement.GetConnectionPoints().Where(p => type == GripType.None || p.Type == type);
            nearElements = GetNearbyElements(connectionPoints);
            ShowConnectionPoints(nearElements.Select(e => e.NearElement), true);
            ShowConnectionPoints(currentlyNear.Where(e => !nearElements.Any(e2 => e.NearElement == e2.NearElement)).Select(e => e.NearElement), false);
            currentlyNear = nearElements;

            // Issue #6 
            // TODO: Again, sort of kludgy.
            UpdateWithNearElementConnectionPoints(nearElements);
            nearElements = nearElements.OrderBy(si => si.AbsDx + si.AbsDy).ToList();    // abs(dx) + abs(dy) as a fast "distance" sorter, no need for sqrt(dx^2 + dy^2)

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
                        if (neardxsign == 0 && neardxsign == 0 && ((delta.X.Abs() >= SNAP_DETACH_VELOCITY || delta.Y.Abs() >= SNAP_DETACH_VELOCITY) ||
                            (isByKeyPress && (neardxsign != deltaxsign || neardysign != deltaysign))))
                        {
                            action = new SnapAction(SnapAction.Action.Detach, selectedElement, type, si.NearElement, si.LineConnectionPoint, nearConnectionPoint, delta);
                            // Disconnect(selectedElement, type);
                            break;
                        }
                        else
                        {
                            // Not already connected?
                            if (neardxsign != 0 || neardysign != 0)
                            {
                                // Remove any current connections.  See issue #41
                                // Disconnect(selectedElement, type);
                                action = new SnapAction(SnapAction.Action.Attach, selectedElement, type, si.NearElement, si.LineConnectionPoint, nearConnectionPoint, new Point(neardx, neardy));
                                //si.NearElement.Connections.Add(new Connection() { ToElement = selectedElement, ToConnectionPoint = si.LineConnectionPoint, ElementConnectionPoint = nearConnectionPoint });
                                //selectedElement.SetConnection(si.LineConnectionPoint.Type, si.NearElement);
                            }
                            else
                            {
                                action = new SnapAction();
                                break;
                            }

                            // delta = new Point(neardx, neardy);
                            // snapped = true;
                            break;
                        }
                    }
                }
            }

            return action;
        }

        public void HideConnectionPoints()
        {
            ShowConnectionPoints(nearElements.Select(e => e.NearElement), false);
            nearElements.Clear();
        }

        /// <summary>
        /// Update the SnapInfo structure with the deltas of the connector's connection point to the first nearby shape connection point found.
        /// </summary>
        protected void UpdateWithNearElementConnectionPoints(List<SnapInfo> nearElements)
        {
            foreach (SnapInfo si in nearElements)
            {
                // TODO: FirstOrDefault, or Where, returning a list of nearby CP's?
                ConnectionPoint nearConnectionPoint = si.NearElement.GetConnectionPoints().FirstOrDefault(cp => cp.Point.IsNear(si.LineConnectionPoint.Point, SNAP_CONNECTION_POINT_RANGE));

                if (nearConnectionPoint != null)
                {
                    Point sourceConnectionPoint = si.LineConnectionPoint.Point;
                    int neardx = nearConnectionPoint.Point.X - sourceConnectionPoint.X;     // calculate to match possible delta sign
                    int neardy = nearConnectionPoint.Point.Y - sourceConnectionPoint.Y;
                    si.NearConnectionPoint = nearConnectionPoint;
                    si.AbsDx = neardx.Abs();
                    si.AbsDy = neardy.Abs();
                }
            }
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

            controller.Elements.Where(e => e != controller.SelectedElements[0] && e.OnScreen() && !e.IsConnector).ForEach(e =>
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
                controller.Redraw(e, CONNECTION_POINT_SIZE, CONNECTION_POINT_SIZE);
            });
        }
    }
}
