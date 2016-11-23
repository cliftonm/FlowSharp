/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

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
        public Point RunningDelta { get { return runningDelta; } }
        public List<SnapAction> SnapActions { get { return snapActions; } }

        protected BaseController controller;
        protected Point runningDelta;
        protected SnapAction currentSnapAction;
        protected List<SnapAction> snapActions = new List<SnapAction>();


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

        public void UpdateRunningDelta(Point delta)
        {
            runningDelta = runningDelta.Add(delta);
        }

        public void Reset()
        {
            snapActions.Clear();
            runningDelta = Point.Empty;
            currentSnapAction = null;
        }

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
                            // Detach:
                            action = new SnapAction(SnapAction.Action.Detach, selectedElement, type, si.NearElement, si.LineConnectionPoint, nearConnectionPoint, delta);
                            break;
                        }
                        else
                        {
                            // Not already connected?
                            if (neardxsign != 0 || neardysign != 0)
                            {
                                // Attach:
                                action = new SnapAction(SnapAction.Action.Attach, selectedElement, type, si.NearElement, si.LineConnectionPoint, nearConnectionPoint, new Point(neardx, neardy));
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

        public bool SnapCheck(GripType gripType, Point delta, Action<Point> update)
        {
            SnapAction action = Snap(gripType, delta);

            if (action != null)
            {
                if (action.SnapType == SnapAction.Action.Attach)
                {
                    runningDelta = runningDelta.Add(action.Delta);
                    update(action.Delta);
                    // Controller.DragSelectedElements(action.Delta);
                    // Don't attach at this point, as this will be handled by the mouse-up action.
                    SetCurrentAction(action);

                }
                else if (action.SnapType == SnapAction.Action.Detach)
                {
                    runningDelta = runningDelta.Add(action.Delta);
                    update(action.Delta);
                    // Controller.DragSelectedElements(action.Delta);
                    // Don't detach at this point, as this will be handled by the mouse-up action.
                    SetCurrentAction(action);
                }
                else // Attached
                {
                    // The mouse move had no affect in detaching because it didn't have sufficient velocity.
                    // The problem here is that the mouse moves, affecting the total delta, but the shape doesn't move.
                    // This affects the computation in the MouseUp handler:
                    // Point delta = CurrentMousePosition.Delta(startedDraggingShapesAt);

                    // ===================
                    // We could set the mouse cursor position, which isn't a bad idea, as it keeps the mouse with the shape:

                    //Controller.Canvas.MouseMove -= HandleMouseMoveEvent;
                    //Cursor.Position = Controller.Canvas.PointToScreen(LastMousePosition);
                    //Application.DoEvents();         // sigh - we need the event to trigger, even though it's unwired.
                    //Controller.Canvas.MouseMove += HandleMouseMoveEvent;

                    // The above really doesn't work well because I think we can get multiple move events, and this only handles the first event.
                    // ===================

                    // ===================
                    // Or we could add a "compensation" accumulator for dealing with the deltas that don't move the shape.
                    // This works better, except the attached compensation has to be stored for each detach.
                    // attachedCompensation = attachedCompensation.Add(delta);

                    // That doesn't work either, as the attachedCompensation is treated as a move even though there is no actual movement of the connector!
                    // ===================

                    // Final implementation is to use the runningDelta instead of the CurrentMouseMosition - startDraggingShapesAt difference.

                    // startedDraggingShapesAt = CurrentMousePosition;
                }
            }

            return action != null;
        }

        public void DoUndoSnapActions(UndoStack undoStack)
        {
            snapActions.ForEachReverse(act => DoUndoSnapAction(undoStack, act));

            if (currentSnapAction != null)
            {
                DoUndoSnapAction(undoStack, currentSnapAction);
            }

            snapActions.Clear();
        }

        protected void DoUndoSnapAction(UndoStack undoStack, SnapAction action)
        {
            SnapAction closureAction = action.Clone();

            // Do/undo/redo as part of of the move group.
            if (closureAction.SnapType == SnapAction.Action.Attach)
            {
                undoStack.UndoRedo("Attach",
                () => closureAction.Attach(),
                () => closureAction.Detach(),
                false);
            }
            else
            {
                undoStack.UndoRedo("Detach",
                () => closureAction.Detach(),
                () => closureAction.Attach(),
                false);
            }
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

        // If no current snap action, set it to the action.
        // Otherwise, if set, we're possibly undoing the last snap action (these are always opposite attach/detach actions),
        // if re-attaching or re-detaching from the connection point.  
        // Lastly, if attaching to a different connection point, buffer the last snap action (which would always be a detach)
        // and set the current snap action to what will always be the attach to another connection point.
        protected void SetCurrentAction(SnapAction action)
        {
            if (currentSnapAction == null)
            {
                currentSnapAction = action;
            }
            else
            {
                // Connecting to a different shape?
                if (action.TargetShape != currentSnapAction.TargetShape
                    // connecting to a different endpoint on the connector?
                    || action.GripType != currentSnapAction.GripType
                    // connecting to a different connection point on the shape?
                    || action.ShapeConnectionPoint != currentSnapAction.ShapeConnectionPoint)
                {
                    snapActions.Add(currentSnapAction);
                    currentSnapAction = action;
                }
                else
                {
                    // User is undoing the last action by re-connecting or disconnecting from the shape to which we just connected / disconnected.
                    currentSnapAction = null;
                }
            }
        }
    }
}
