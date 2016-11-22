/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FlowSharpLib
{
    public class MouseAction
    {
        public MouseController.MouseEvent MouseEvent { get; }
        public Point MousePosition { get; }
        public MouseButtons Buttons { get; }
        public MouseEventArgs MouseEventArgs { get; }

        public MouseAction(MouseController.MouseEvent mouseEvent, MouseEventArgs args)
        {
            MouseEvent = mouseEvent;
            MouseEventArgs = args;
            MousePosition = args.Location;
            // Buttons = buttons;
        }
    }

    public class MouseRouter
    {
        public MouseController.RouteName RouteName { get; set; }
        public MouseController.MouseEvent MouseEvent { get; set; }
        public Func<bool> Condition { get; set; }
        public Action<MouseEventArgs> Action { get; set; }
        public Action Else { get; set; }
        public Action Debug { get; set; }
    }

    public class MouseController
    {
        public event EventHandler<MouseEventArgs> MouseClick;

        // State information:
        public Point LastMousePosition { get; set; }
        public Point CurrentMousePosition { get; set; }
        public MouseButtons CurrentButtons { get; set; }
        public bool DraggingSurface { get; set; }
        public bool DraggingShapes { get; set; }
        public bool DraggingAnchor { get; set; }
        public bool DraggingOccurred { get; set; }
        public bool DraggingSurfaceOccurred { get; set; }
        public bool SelectingShapes { get; set; }
        public GraphicElement HoverShape { get; set; }
        public ShapeAnchor SelectedAnchor { get; set; }
        public GraphicElement SelectionBox { get; set; }
        public bool DraggingSelectionBox { get; set; }
        public Point StartSelectionPosition { get; set; }

        public BaseController Controller { get; protected set; }

        protected List<MouseRouter> router;
        protected List<GraphicElement> justAddedShape = new List<GraphicElement>();
        protected Point startedDraggingShapesAt;
        // protected Point attachedCompensation;
        protected Point runningDelta;
        protected SnapAction currentSnapAction;
        protected List<SnapAction> snapActions = new List<SnapAction>();

        public enum MouseEvent
        {
            MouseDown,
            MouseUp,
            MouseMove,
        }

        public enum RouteName
        {
            FireMouseClickEvent,
            CanvasFocus,
            StartDragSurface,
            EndDragSurface,
            EndDragSurfaceWithDeselect,
            DragSurface,
            StartDragSelectionBox,
            EndDragSelectionBox,
            DragSelectionBox,
            StartShapeDrag,
            EndShapeDrag,
            StartAnchorDrag,
            EndAnchorDrag,
            DragShapes,
            DragAnchor,
            HoverOverShape,
            ShowAnchors,
            ShowAnchorCursor,
            ClearAnchorCursor,
            HideAnchors,
            SelectSingleShapeMouseUp,
            SelectSingleShapeMouseDown,
            SelectSingleGroupedShape,
            AddSelectedShape,
            RemoveSelectedShape,
        }

        public MouseController(BaseController controller)
        {
            Controller = controller;
            router = new List<MouseRouter>();
        }

        public void HookMouseEvents()
        {
            Controller.Canvas.MouseDown += (sndr, args) => HandleEvent(new MouseAction(MouseEvent.MouseDown, args));
            Controller.Canvas.MouseUp += (sndr, args) => HandleEvent(new MouseAction(MouseEvent.MouseUp, args));
            Controller.Canvas.MouseMove += HandleMouseMoveEvent;        // Actual instance, so we can detach it and re-attach it for the "Attached" condition when doing a snap check.
        }

        protected void HandleMouseMoveEvent(object sender, MouseEventArgs args)
        {
            HandleEvent(new MouseAction(MouseEvent.MouseMove, args));
        }

        // After new/open action, clear state.
        public void ClearState()
        {
            DraggingShapes = false;
            DraggingAnchor = false;
            DraggingOccurred = false;
            DraggingSurfaceOccurred = false;
            SelectingShapes = false;
            DraggingSurface = false;
            HoverShape = null;
        }

        public void ShapeDeleted(GraphicElement el)
        {
            if (HoverShape == el)
            {
                DraggingSurface = false;
                HoverShape.ShowAnchors = false;
                HoverShape = null;
            }
        }

        public virtual void InitializeBehavior()
        {
            // Any mouse down fires the MouseClick event for external handling.
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.FireMouseClickEvent,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => true,
                Action = (mouseEventArgs) =>
                {
                    // So Ctrl+V paste works, as keystroke is intercepted only when canvas panel has focus.
                    MouseClick.Fire(this, mouseEventArgs);
                }
            });

            router.Add(new MouseRouter()
            {
                RouteName = RouteName.CanvasFocus,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => true,
                Action = (_) =>
                {
                    // So Ctrl+V paste works, as keystroke is intercepted only when canvas panel has focus.
                    Controller.Canvas.Focus();
                }
            });

            // DRAG SURFACE ROUTES:

            // Start drag surface:
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.StartDragSurface,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => !Controller.IsRootShapeSelectable(CurrentMousePosition) && CurrentButtons == MouseButtons.Left,
                Action = (_) =>
                {
                    DraggingSurface = true;
                    DraggingSurfaceOccurred = false;
                }
            });

            // End drag surface with no dragging, which deselects all selected shapes
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.EndDragSurfaceWithDeselect,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => DraggingSurface && !DraggingSurfaceOccurred,
                Action = (_) =>
                {
                    DraggingSurface = false;
                    Controller.Canvas.Cursor = Cursors.Arrow;
                    List<GraphicElement> selectedShapes = Controller.SelectedElements.ToList();

                    if (selectedShapes.Count != 0)
                    {
                        Controller.UndoStack.UndoRedo("Canvas",
                            () =>
                            {
                                Controller.DeselectCurrentSelectedElements();
                            },
                            () =>
                            {
                                Controller.DeselectCurrentSelectedElements();
                                Controller.SelectElements(selectedShapes);
                            });
                    }
                }
            });

            // End drag surface when dragging occurred, selected shapes stay selected.
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.EndDragSurface,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => DraggingSurface && DraggingSurfaceOccurred,
                Action = (_) =>
                {
                    DraggingSurface = false;
                    DraggingSurfaceOccurred = false;
                    Controller.Canvas.Cursor = Cursors.Arrow;
                }
            });

            // Drag surface:
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.DragSurface,
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => DraggingSurface,
                Action = (_) =>
                {
                    DraggingSurfaceOccurred = true;
                    DragCanvas();
                }
            });

            // SHAPE DRAGGING ROUTES:

            // Start shape drag:
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.StartShapeDrag,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => Controller.IsRootShapeSelectable(CurrentMousePosition) &&
                    CurrentButtons == MouseButtons.Left &&
                    Controller.GetRootShapeAt(CurrentMousePosition).GetAnchors().FirstOrDefault(a => a.Near(CurrentMousePosition)) == null &&
                    !Controller.IsChildShapeSelectable(CurrentMousePosition),       // can't drag a grouped shape
                Action = (_) =>
                {
                    snapActions.Clear();
                    // attachedCompensation = Point.Empty;
                    runningDelta = Point.Empty;
                    Controller.DeselectGroupedElements();
                    DraggingShapes = true;
                    startedDraggingShapesAt = CurrentMousePosition;
                },
            });

            // Start anchor drag:
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.StartAnchorDrag,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => Controller.IsRootShapeSelectable(CurrentMousePosition) &&
                    CurrentButtons == MouseButtons.Left &&
                    Controller.GetRootShapeAt(CurrentMousePosition).GetAnchors().FirstOrDefault(a => a.Near(CurrentMousePosition)) != null,
                Action = (_) =>
                {
                    snapActions.Clear();
                    runningDelta = Point.Empty;
                    DraggingAnchor = true;
                    SelectedAnchor = HoverShape.GetAnchors().First(a => a.Near(CurrentMousePosition));
                },
            });

            // End shape dragging:
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.EndShapeDrag,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => DraggingShapes,
                Action = (_) =>
                {
                    DoUndoSnapActions();

                    if (runningDelta != Point.Empty)
                    {
                        Point delta = runningDelta;     // for closure

                        Controller.UndoStack.UndoRedo("ShapeMove",
                            () => { },      // Our "do" action is actually nothing, since all the "doing" has been done.
                            () =>           // Undo
                            {
                            Controller.DragSelectedElements(delta.ReverseDirection());
                            },
                            true,       // We finish the move.
                            () =>           // Redo
                            {
                                Controller.DragSelectedElements(delta);
                            });
                    }

                    Controller.SnapController.HideConnectionPoints();
                    currentSnapAction = null;
                    DraggingShapes = false;
                    // DraggingOccurred = false;        / Will be cleared by RemoveSelectedShape but this is order dependent!  TODO: Fix this somehow! :)
                    DraggingAnchor = false;
                    SelectedAnchor = null;
                    Controller.Canvas.Cursor = Cursors.Arrow;
                }
            });

            // End anchor dragging:
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.EndAnchorDrag,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => DraggingAnchor,
                Action = (_) =>
                {
                    DoUndoSnapActions();

                    if (runningDelta != Point.Empty)
                    {
                        Point delta = runningDelta;     // for closure
                        GraphicElement hoverShape = HoverShape;
                        ShapeAnchor selectedAnchor = SelectedAnchor;

                        Controller.UndoStack.UndoRedo("AnchorMove",
                            () => { },      // Our "do" action is actually nothing, since all the "doing" has been done.
                            () =>           // Undo
                            {
                                hoverShape.UpdateSize(selectedAnchor, delta.ReverseDirection());
                            },
                            true,       // We finish the anchor drag.
                            () =>           // Redo
                            {
                                hoverShape.UpdateSize(selectedAnchor, delta);
                            });
                    }

                    Controller.SnapController.HideConnectionPoints();
                    currentSnapAction = null;
                    DraggingShapes = false;
                    // DraggingOccurred = false;        / Will be cleared by RemoveSelectedShape but this is order dependent!  TODO: Fix this somehow! :)
                    DraggingAnchor = false;
                    SelectedAnchor = null;
                    Controller.Canvas.Cursor = Cursors.Arrow;
                }
            });

            // Drag shapes:
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.DragShapes,
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => DraggingShapes &&
                    HoverShape != null &&
                    HoverShape.GetAnchors().FirstOrDefault(a => a.Near(CurrentMousePosition)) == null,
                Action = (_) =>
                {
                    DragShapes();
                    DraggingOccurred = true;
                },
            });

            // Drag anchor:
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.DragAnchor,
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => HoverShape != null && DraggingAnchor,
                Action = (_) =>
                {
                    DragAnchor();
                },
            });

            // HOVER ROUTES

            // Show anchors when hovering over a shape
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.HoverOverShape,
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => !DraggingSurface && !DraggingShapes && !SelectingShapes && HoverShape == null &&
                    CurrentButtons == MouseButtons.None &&
                    Controller.IsRootShapeSelectable(CurrentMousePosition) &&
                    Controller.GetRootShapeAt(CurrentMousePosition).Parent == null, // no anchors for grouped children.
                Action = (_) => ShowAnchors(),
            });

            // Change anchors when hover shape changes
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.ShowAnchors,
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => !DraggingSurface && !DraggingShapes && !SelectingShapes && HoverShape != null &&
                    CurrentButtons == MouseButtons.None &&
                    Controller.IsRootShapeSelectable(CurrentMousePosition) &&
                    HoverShape != Controller.GetRootShapeAt(CurrentMousePosition) &&
                    Controller.GetRootShapeAt(CurrentMousePosition).Parent == null, // no anchors for grouped children.
                Action = (_) => ChangeAnchors(),
            });

            // Hide anchors when not hovering over a shape
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.HideAnchors,
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => !DraggingSurface && !DraggingShapes && !SelectingShapes && HoverShape != null &&
                    CurrentButtons == MouseButtons.None &&
                    !Controller.IsRootShapeSelectable(CurrentMousePosition),
                Action = (_) => HideAnchors(),
            });

            // Show cursor when hovering over an anchor
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.ShowAnchorCursor,
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => !DraggingSurface && !DraggingShapes && !SelectingShapes && !DraggingAnchor && HoverShape != null &&
                    HoverShape.GetAnchors().FirstOrDefault(a => a.Near(CurrentMousePosition)) != null,
                Action = (_) => SetAnchorCursor(),
            });

            // Clear cursor when hovering over an anchor
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.ClearAnchorCursor,
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => !DraggingSurface && !DraggingShapes && !SelectingShapes && HoverShape != null &&
                    HoverShape.GetAnchors().FirstOrDefault(a => a.Near(CurrentMousePosition)) == null,
                Action = (_) => ClearAnchorCursor(),
            });

            // SHAPE SELECTION

            // Select a shape
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.SelectSingleShapeMouseDown,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => Controller.IsRootShapeSelectable(CurrentMousePosition) &&
                    !Controller.IsChildShapeSelectable(CurrentMousePosition) &&
                    !Controller.IsMultiSelect() &&
                    !Controller.SelectedElements.Contains(Controller.GetRootShapeAt(CurrentMousePosition)),
                Action = (_) => SelectSingleRootShape()
            });

            // Select a single grouped shape:
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.SelectSingleGroupedShape,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => Controller.IsChildShapeSelectable(CurrentMousePosition) &&
                    !Controller.IsMultiSelect() &&
                    !Controller.SelectedElements.Contains(Controller.GetChildShapeAt(CurrentMousePosition)),
                Action = (_) => SelectSingleChildShape()
            });

            // Select a single shape
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.SelectSingleShapeMouseUp,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => Controller.IsRootShapeSelectable(CurrentMousePosition) &&
                    !Controller.IsChildShapeSelectable(CurrentMousePosition) &&     // Don't deselect grouped shape on mouse up (as in, don't select groupbox)
                    !Controller.IsMultiSelect() &&
                    !DraggingOccurred && !DraggingSelectionBox,
                Action = (_) => SelectSingleRootShape()
            });

            // Add another shape to selection list
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.AddSelectedShape,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => Controller.IsRootShapeSelectable(CurrentMousePosition) &&
                    Controller.IsMultiSelect() && !DraggingSelectionBox &&
                    !Controller.SelectedElements.Contains(Controller.GetRootShapeAt(CurrentMousePosition)),
                Action = (_) => AddShapeToSelectionList(),
            });

            // Remove shape from selection list
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.RemoveSelectedShape,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => Controller.IsRootShapeSelectable(CurrentMousePosition) &&
                    Controller.IsMultiSelect() && !DraggingSelectionBox &&
                    // TODO: Would nice to avoid multiple GetShapeAt calls when processing conditions.  And not just here.
                    Controller.SelectedElements.Contains(Controller.GetRootShapeAt(CurrentMousePosition)) &&
                    !justAddedShape.Contains(Controller.GetRootShapeAt(CurrentMousePosition)) &&
                    !DraggingOccurred,
                Action = (_) => RemoveShapeFromSelectionList(),
                Else = () =>
                {
                    justAddedShape.Clear();
                    DraggingOccurred = false;
                },
                Debug = () =>
                {
                    Trace.WriteLine("Route:IsRootShapeSelectable: " + Controller.IsRootShapeSelectable(CurrentMousePosition));
                    Trace.WriteLine("Route:IsMultiSelect: " + Controller.IsMultiSelect());
                    Trace.WriteLine("Route:!DraggingSelectionBox: " + !DraggingSelectionBox);
                    Trace.WriteLine("Route:SelectedElements.ContainsShape: " + Controller.SelectedElements.Contains(Controller.GetRootShapeAt(CurrentMousePosition)));
                    Trace.WriteLine("Route:!justShapeAdded: " + !justAddedShape.Contains(Controller.GetRootShapeAt(CurrentMousePosition)));
                    Trace.WriteLine("Route:!DraggingOccurred: " + !DraggingOccurred);
                }
            });

            // SELECTION BOX

            // Start selection box
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.StartDragSelectionBox,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => !Controller.IsRootShapeSelectable(CurrentMousePosition) && CurrentButtons == MouseButtons.Right,
                Action = (_) =>
                {
                    DraggingSelectionBox = true;
                    StartSelectionPosition = CurrentMousePosition;
                    CreateSelectionBox();                
                },
            });

            // End selection box
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.EndDragSelectionBox,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => DraggingSelectionBox,
                Action = (_) =>
                {
                    DraggingSelectionBox = false;
                    SelectShapesInSelectionBox();
                }
            });

            // Drag selection box
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.DragSelectionBox,
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => DraggingSelectionBox,
                Action = (_) => DragSelectionBox(),
            });
        }

        protected bool MouseHasReallyMoved()
        {
            return CurrentMousePosition.Delta(LastMousePosition) != Point.Empty;
        }

        protected virtual void HandleEvent(MouseAction action)
        {
            CurrentMousePosition = action.MousePosition;
            CurrentButtons = Control.MouseButtons;
            // Issue #39: Mouse Move event fires even for button press when mouse hasn't moved!
            IEnumerable<MouseRouter> routes = router.Where(r => (action.MouseEvent != MouseEvent.MouseMove && r.MouseEvent == action.MouseEvent)
                || ((action.MouseEvent == MouseEvent.MouseMove && r.MouseEvent == action.MouseEvent && MouseHasReallyMoved())));

            routes.ForEach(r =>
            {
                r.Debug?.Invoke();

                // Test condition every time after executing a route handler, as the handler may change state for the next condition.
                if (r.Condition())
                {
                    Trace.WriteLine("Route:" + r.RouteName.ToString());
                    r.Action(action.MouseEventArgs);
                }
                else
                {
                    r.Else?.Invoke();
                }
            });

            LastMousePosition = CurrentMousePosition;
        }

        protected virtual void DragCanvas()
        {
            Point delta = CurrentMousePosition.Delta(LastMousePosition);
            Controller.Canvas.Cursor = Cursors.SizeAll;
            // Pick up every object on the canvas and move it.
            // This does not "move" the grid.
            Controller.MoveAllElements(delta);

            // Conversely, we redraw the grid and invalidate, which forces all the elements to redraw.
            //canvas.Drag(delta);
            //elements.ForEach(el => el.Move(delta));
            //canvas.Invalidate();
        }

        protected void ShowAnchors()
        {
            GraphicElement el = Controller.GetRootShapeAt(CurrentMousePosition);
            el.ShowAnchors = true;
            Controller.Redraw(el);
            HoverShape = el;
            Controller.SetAnchorCursor(el);
        }

        protected void ChangeAnchors()
        {
            HoverShape.ShowAnchors = false;
            Controller.Redraw(HoverShape);
            HoverShape = Controller.GetRootShapeAt(CurrentMousePosition);
            HoverShape.ShowAnchors = true;
            Controller.Redraw(HoverShape);
            Controller.SetAnchorCursor(HoverShape);
        }

        protected void HideAnchors()
        {
            HoverShape.ShowAnchors = false;
            Controller.Redraw(HoverShape);
            Controller.Canvas.Cursor = Cursors.Arrow;
            HoverShape = null;
        }

        protected void SelectSingleRootShape()
        {
            // Preserve for undo:
            List<GraphicElement> selectedShapes = Controller.SelectedElements.ToList();
            GraphicElement el = Controller.GetRootShapeAt(CurrentMousePosition);

            if (selectedShapes.Count != 1 || !selectedShapes.Contains(el))
            {
                Controller.UndoStack.UndoRedo("Select Root " + el.ToString(),
                    () =>
                    {
                        Controller.DeselectCurrentSelectedElements();
                        Controller.SelectElement(el);
                    },
                    () =>
                    {
                        Controller.DeselectCurrentSelectedElements();
                        Controller.SelectElements(selectedShapes);
                    });
            }
        }

        protected void SelectSingleChildShape()
        {
            // Preserve for undo:
            List<GraphicElement> selectedShapes = Controller.SelectedElements.ToList();
            GraphicElement el = Controller.GetChildShapeAt(CurrentMousePosition);
            Controller.UndoStack.UndoRedo("Select Child " + el.ToString(),
                () =>
                {
                    Controller.DeselectCurrentSelectedElements();
                    Controller.SelectElement(el);
                },
                () =>
                {
                    Controller.DeselectCurrentSelectedElements();
                    Controller.SelectElements(selectedShapes);
                });
        }

        protected void AddShapeToSelectionList()
        {
            // Preserve for undo:
            List<GraphicElement> selectedShapes = Controller.SelectedElements.ToList();

            GraphicElement el = Controller.GetRootShapeAt(CurrentMousePosition);
            Controller.UndoStack.UndoRedo("Select " + el.ToString(),
                () =>
                {
                    Controller.DeselectGroupedElements();
                    Controller.SelectElement(el);
                    justAddedShape.Add(el);
                },
                () =>
                {
                    Controller.DeselectCurrentSelectedElements();
                    Controller.SelectElements(selectedShapes);
                });
        }

        protected void RemoveShapeFromSelectionList()
        {
            GraphicElement el = Controller.GetRootShapeAt(CurrentMousePosition);
            Controller.UndoStack.UndoRedo("Deselect " + el.ToString(),
                () =>
                {
                    Controller.DeselectElement(el);
                },
                () =>
                {
                    Controller.SelectElement(el);
                });
        }

        protected void DragShapes()
        {
            Controller.Canvas.Cursor = Cursors.SizeAll;
            Point delta = CurrentMousePosition.Delta(LastMousePosition);

            if (Controller.SelectedElements.Count == 1 && Controller.SelectedElements[0].IsConnector)
            {
                // Check both ends of any connector being moved.
                if (!SnapCheck(GripType.Start, delta, (snapDelta) => Controller.DragSelectedElements(snapDelta)))
                {
                    if (!SnapCheck(GripType.End, delta, (snapDelta) => Controller.DragSelectedElements(snapDelta)))
                    {
                        Controller.DragSelectedElements(delta);
                        runningDelta = runningDelta.Add(delta);
                    }
                }
            }
            else
            {
                Controller.DragSelectedElements(delta);
                runningDelta = runningDelta.Add(delta);
            }
        }

        protected bool SnapCheck(GripType gripType, Point delta, Action<Point> update)
        {
            SnapAction action = Controller.SnapController.Snap(gripType, delta);

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

        // If no current snap action, set it to the action.
        // Otherwise, if set, we're undoing the last snap action (these are always opposite attach/detach actions),
        // so set it back to null.
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

        protected void ClearAnchorCursor()
        {
            Controller.Canvas.Cursor = Cursors.Arrow;
        }

        protected void SetAnchorCursor()
        {
            ShapeAnchor anchor = HoverShape.GetAnchors().FirstOrDefault(a => a.Near(CurrentMousePosition));

            // Hover shape could have changed as we move from a shape to a connector's anchor.
            if (anchor != null)
            {
                Controller.Canvas.Cursor = anchor.Cursor;
            }
        }

        protected void DragAnchor()
        {
            Point delta = CurrentMousePosition.Delta(LastMousePosition);
            GraphicElement hoverShape = HoverShape;
            ShapeAnchor selectedAnchor = SelectedAnchor;

            if (!SnapCheck(selectedAnchor.Type, delta, (snapDelta) => hoverShape.UpdateSize(selectedAnchor, snapDelta)))
            {
                hoverShape.UpdateSize(selectedAnchor, delta);
                runningDelta = runningDelta.Add(delta);
            }
        }

        protected void CreateSelectionBox()
        {
            SelectionBox = new Box(Controller.Canvas);
            SelectionBox.BorderPen.Color = Color.Gray;
            SelectionBox.FillBrush.Color = Color.Transparent;
            SelectionBox.DisplayRectangle = new Rectangle(StartSelectionPosition, new Size(1, 1));
            Controller.Insert(SelectionBox);
        }

        protected void SelectShapesInSelectionBox()
        {
            Controller.DeleteElement(SelectionBox);
            List<GraphicElement> selectedElements = new List<GraphicElement>();
            List<GraphicElement> previouslySelectedElements = Controller.SelectedElements.ToList();

            Controller.Elements.Where(e => !selectedElements.Contains(e) && e.Parent == null && SelectionBox.DisplayRectangle.Contains(e.UpdateRectangle)).ForEach((e) =>
            {
                selectedElements.Add(e);
            });

            Controller.UndoStack.UndoRedo("Group Select",
                () =>
                {
                    Controller.DeselectCurrentSelectedElements();
                    Controller.SelectElements(selectedElements);
                },
                () =>
                {
                    Controller.DeselectCurrentSelectedElements();
                    Controller.SelectElements(previouslySelectedElements);
                });
            // Why was this here?
            // Controller.Canvas.Invalidate();
        }

        protected void DragSelectionBox()
        {
            // Normalize the rectangle to a top-left, bottom-right rectangle.
            int x = CurrentMousePosition.X.Min(StartSelectionPosition.X);
            int y = CurrentMousePosition.Y.Min(StartSelectionPosition.Y);
            int w = (CurrentMousePosition.X - StartSelectionPosition.X).Abs();
            int h = (CurrentMousePosition.Y - StartSelectionPosition.Y).Abs();
            Rectangle newRect = new Rectangle(x, y, w, h);
            Point delta = CurrentMousePosition.Delta(LastMousePosition);
            Controller.UpdateDisplayRectangle(SelectionBox, newRect, delta);
        }

        protected void DoUndoSnapActions()
        {
            snapActions.ForEachReverse(act => DoUndoSnapAction(act));

            if (currentSnapAction != null)
            {
                DoUndoSnapAction(currentSnapAction);
            }

            snapActions.Clear();
        }

        protected void DoUndoSnapAction(SnapAction action)
        {
            SnapAction closureAction = action.Clone();

            // Do/undo/redo as part of of the move group.
            if (closureAction.SnapType == SnapAction.Action.Attach)
            {
                Controller.UndoStack.UndoRedo("Attach",
                () => closureAction.Attach(),
                () => closureAction.Detach(),
                false);
            }
            else
            {
                Controller.UndoStack.UndoRedo("Detach",
                () => closureAction.Detach(),
                () => closureAction.Attach(),
                false);
            }
        }
    }
}
