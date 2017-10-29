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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharpMouseControllerService
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
        protected Point LastMousePosition { get; set; }
        protected Point CurrentMousePosition { get; set; }
        protected MouseButtons CurrentButtons { get; set; }
        protected bool DraggingSurface { get; set; }
        protected bool DraggingShapes { get; set; }
        protected bool DraggingAnchor { get; set; }
        protected bool DraggingOccurred { get; set; }
        protected bool DraggingSurfaceOccurred { get; set; }
        protected bool SelectingShapes { get; set; }
        protected GraphicElement HoverShape { get; set; }
        protected ShapeAnchor SelectedAnchor { get; set; }
        protected GraphicElement SelectionBox { get; set; }
        protected bool DraggingSelectionBox { get; set; }
        protected Point StartSelectionPosition { get; set; }

        protected List<MouseRouter> router;
        protected List<GraphicElement> justAddedShape = new List<GraphicElement>();
        protected Point startedDraggingShapesAt;
        protected IServiceManager serviceManager;

        protected int doubleClickCounter = 0;

        public enum MouseEvent
        {
            MouseDown,
            MouseUp,
            MouseMove,
            MouseDoubleClick,
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
            EditShapeText,
        }

        public MouseController(IServiceManager serviceManager)
        {
            this.serviceManager = serviceManager;
            router = new List<MouseRouter>();
        }

        public void HookMouseEvents(BaseController controller)
        {
            controller.Canvas.MouseDown += (sndr, args) => HandleEvent(new MouseAction(MouseEvent.MouseDown, args));
            controller.Canvas.MouseUp += (sndr, args) => HandleEvent(new MouseAction(MouseEvent.MouseUp, args));
            controller.Canvas.MouseMove += HandleMouseMoveEvent;        // Actual instance, so we can detach it and re-attach it for the "Attached" condition when doing a snap check.
            controller.Canvas.MouseDoubleClick += (sndr, args) => HandleEvent(new MouseAction(MouseEvent.MouseDoubleClick, args));
        }

        protected void HandleMouseMoveEvent(object sender, MouseEventArgs args)
        {
            HandleEvent(new MouseAction(MouseEvent.MouseMove, args));
        }

        // After new/open action, clear state.  State also clears when document is changed.
        public void ClearState()
        {
            DraggingShapes = false;
            DraggingAnchor = false;
            DraggingOccurred = false;
            DraggingSurfaceOccurred = false;
            SelectingShapes = false;
            DraggingSurface = false;
            HoverShape = null;
            justAddedShape.Clear();
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
                // Condition = () => true,
                Condition = () => !serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsRootShapeSelectable(CurrentMousePosition) &&
                    !serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsChildShapeSelectable(CurrentMousePosition) &&
                    !serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsMultiSelect(),
                Action = (_) =>
                {
                    // So Ctrl+V paste works, as keystroke is intercepted only when canvas panel has focus.
                    BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
                    canvasController.Canvas.Focus();
                    serviceManager.IfExists<IFlowSharpPropertyGridService>(pgs => pgs.ShowProperties(new CanvasProperties(canvasController.Canvas)));
                },
                Else = () =>
                {
                    // So Ctrl+V paste works, as keystroke is intercepted only when canvas panel has focus.
                    BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
                    canvasController.Canvas.Focus();
                }
            });

            // DRAG SURFACE ROUTES:

            // Start drag surface:
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.StartDragSurface,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => !serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsRootShapeSelectable(CurrentMousePosition) && CurrentButtons == MouseButtons.Left,
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
                    BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
                    controller.Canvas.Cursor = Cursors.Arrow;
                    List<GraphicElement> selectedShapes = controller.SelectedElements.ToList();

                    if (selectedShapes.Count != 0)
                    {
                        controller.UndoStack.UndoRedo("Canvas",
                            () =>
                            {
                                controller.DeselectCurrentSelectedElements();
                            },
                            () =>
                            {
                                controller.DeselectCurrentSelectedElements();
                                controller.SelectElements(selectedShapes);
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
                    BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
                    controller.Canvas.Cursor = Cursors.Arrow;
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
                Condition = () => serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsRootShapeSelectable(CurrentMousePosition) &&
                    CurrentButtons == MouseButtons.Left &&
                    serviceManager.Get<IFlowSharpCanvasService>().ActiveController.GetRootShapeAt(CurrentMousePosition).GetAnchors().FirstOrDefault(a => a.Near(CurrentMousePosition)) == null &&
                    !serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsChildShapeSelectable(CurrentMousePosition),       // can't drag a grouped shape
                Action = (_) =>
                {
                    BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
                    controller.SnapController.Reset();
                    // Deselect grouped elements so that those selected in a grouping and those not in a grouping results in the grouped elements being
                    // removed from the selection criteria, as grouped elements cannot be moved with non-grouped elements.
                    controller.DeselectGroupedElements();
                    DraggingShapes = true;
                    startedDraggingShapesAt = CurrentMousePosition;
                },
            });

            // Start anchor drag:
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.StartAnchorDrag,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsRootShapeSelectable(CurrentMousePosition) &&
                    CurrentButtons == MouseButtons.Left &&
                    serviceManager.Get<IFlowSharpCanvasService>().ActiveController.GetRootShapeAt(CurrentMousePosition).GetAnchors().FirstOrDefault(a => a.Near(CurrentMousePosition)) != null,
                Action = (_) =>
                {
                    BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
                    controller.SnapController.Reset();
                    DraggingAnchor = true;
                    HoverShape = serviceManager.Get<IFlowSharpCanvasService>().ActiveController.GetRootShapeAt(CurrentMousePosition);
                    SelectedAnchor = HoverShape.GetAnchors().First(a => a.Near(CurrentMousePosition));
                },
            });

            // End shape dragging:
            router.Add(new MouseRouter()
            {
                // TODO: Similar to EndAnchorDrag and Toolbox.OnMouseUp
                RouteName = RouteName.EndShapeDrag,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => DraggingShapes,
                Action = (_) =>
                {
                    BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
                    controller.SnapController.DoUndoSnapActions(controller.UndoStack);

                    if (controller.SnapController.RunningDelta != Point.Empty)
                    {
                        Point delta = controller.SnapController.RunningDelta;     // for closure

                        controller.UndoStack.UndoRedo("ShapeMove",
                            () => { },      // Our "do" action is actually nothing, since all the "doing" has been done.
                            () =>           // Undo
                            {
                                controller.DragSelectedElements(delta.ReverseDirection());
                            },
                            true,       // We finish the move.
                            () =>           // Redo
                            {
                                controller.DragSelectedElements(delta);
                            });
                    }

                    controller.SnapController.HideConnectionPoints();
                    controller.SnapController.Reset();
                    DraggingShapes = false;
                    // DraggingOccurred = false;        / Will be cleared by RemoveSelectedShape but this is order dependent!  TODO: Fix this somehow! :)
                    DraggingAnchor = false;
                    SelectedAnchor = null;
                    controller.Canvas.Cursor = Cursors.Arrow;
                }
            });

            // End anchor dragging:
            router.Add(new MouseRouter()
            {
                // TODO: Similar to EndShapeDrag and Toolbox.OnMouseUp
                RouteName = RouteName.EndAnchorDrag,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => DraggingAnchor,
                Action = (_) =>
                {
                    BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
                    controller.SnapController.DoUndoSnapActions(controller.UndoStack);

                    if (controller.SnapController.RunningDelta != Point.Empty)
                    {
                        Point delta = controller.SnapController.RunningDelta;     // for closure
                        GraphicElement hoverShape = HoverShape;
                        ShapeAnchor selectedAnchor = SelectedAnchor;

                        controller.UndoStack.UndoRedo("AnchorMove",
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

                    controller.SnapController.HideConnectionPoints();
                    controller.SnapController.Reset();
                    DraggingShapes = false;
                    // DraggingOccurred = false;        / Will be cleared by RemoveSelectedShape but this is order dependent!  TODO: Fix this somehow! :)
                    DraggingAnchor = false;
                    SelectedAnchor = null;
                    controller.Canvas.Cursor = Cursors.Arrow;
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
                    serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsRootShapeSelectable(CurrentMousePosition) &&
                    serviceManager.Get<IFlowSharpCanvasService>().ActiveController.GetRootShapeAt(CurrentMousePosition).Parent == null, // no anchors for grouped children.
                Action = (_) => ShowAnchors(),
            });

            // Change anchors when hover shape changes
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.ShowAnchors,
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => !DraggingSurface && !DraggingShapes && !SelectingShapes && HoverShape != null &&
                    CurrentButtons == MouseButtons.None &&
                    serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsRootShapeSelectable(CurrentMousePosition) &&
                    HoverShape != serviceManager.Get<IFlowSharpCanvasService>().ActiveController.GetRootShapeAt(CurrentMousePosition) &&
                    serviceManager.Get<IFlowSharpCanvasService>().ActiveController.GetRootShapeAt(CurrentMousePosition).Parent == null, // no anchors for grouped children.
                Action = (_) => ChangeAnchors(),
            });

            // Hide anchors when not hovering over a shape
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.HideAnchors,
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => !DraggingSurface && !DraggingShapes && !SelectingShapes && HoverShape != null &&
                    CurrentButtons == MouseButtons.None &&
                    !serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsRootShapeSelectable(CurrentMousePosition),
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
                Condition = () => serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsRootShapeSelectable(CurrentMousePosition) &&
                    !serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsChildShapeSelectable(CurrentMousePosition) &&
                    !serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsMultiSelect() &&
                    !serviceManager.Get<IFlowSharpCanvasService>().ActiveController.SelectedElements.Contains(serviceManager.Get<IFlowSharpCanvasService>().ActiveController.GetRootShapeAt(CurrentMousePosition)),
                Action = (_) => SelectSingleRootShape()
            });

            // Select a single grouped shape:
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.SelectSingleGroupedShape,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsChildShapeSelectable(CurrentMousePosition) &&
                    !serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsMultiSelect() &&
                    !serviceManager.Get<IFlowSharpCanvasService>().ActiveController.SelectedElements.Contains(serviceManager.Get<IFlowSharpCanvasService>().ActiveController.GetChildShapeAt(CurrentMousePosition)),
                Action = (_) => SelectSingleChildShape()
            });

            // Select a single shape
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.SelectSingleShapeMouseUp,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsRootShapeSelectable(CurrentMousePosition) &&
                    !serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsChildShapeSelectable(CurrentMousePosition) &&     // Don't deselect grouped shape on mouse up (as in, don't select groupbox)
                    !serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsMultiSelect() &&
                    !DraggingOccurred && !DraggingSelectionBox,
                Action = (_) => SelectSingleRootShape()
            });

            // Add another shape to selection list
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.AddSelectedShape,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsRootShapeSelectable(CurrentMousePosition) &&
                    serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsMultiSelect() && !DraggingSelectionBox &&
                    !serviceManager.Get<IFlowSharpCanvasService>().ActiveController.SelectedElements.Contains(serviceManager.Get<IFlowSharpCanvasService>().ActiveController.GetRootShapeAt(CurrentMousePosition)),
                Action = (_) => AddShapeToSelectionList(),
            });

            // Remove shape from selection list
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.RemoveSelectedShape,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsRootShapeSelectable(CurrentMousePosition) &&
                    serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsMultiSelect() && !DraggingSelectionBox &&
                    // TODO: Would nice to avoid multiple GetShapeAt calls when processing conditions.  And not just here.
                    serviceManager.Get<IFlowSharpCanvasService>().ActiveController.SelectedElements.Contains(serviceManager.Get<IFlowSharpCanvasService>().ActiveController.GetRootShapeAt(CurrentMousePosition)) &&
                    !justAddedShape.Contains(serviceManager.Get<IFlowSharpCanvasService>().ActiveController.GetRootShapeAt(CurrentMousePosition)) &&
                    !DraggingOccurred,
                Action = (_) => RemoveShapeFromSelectionList(),
                Else = () =>
                {
                    justAddedShape.Clear();
                    DraggingOccurred = false;
                },
                Debug = () =>
                {
                    Trace.WriteLine("Route:IsRootShapeSelectable: " + serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsRootShapeSelectable(CurrentMousePosition));
                    Trace.WriteLine("Route:IsMultiSelect: " + serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsMultiSelect());
                    Trace.WriteLine("Route:!DraggingSelectionBox: " + !DraggingSelectionBox);
                    Trace.WriteLine("Route:SelectedElements.ContainsShape: " + serviceManager.Get<IFlowSharpCanvasService>().ActiveController.SelectedElements.Contains(serviceManager.Get<IFlowSharpCanvasService>().ActiveController.GetRootShapeAt(CurrentMousePosition)));
                    Trace.WriteLine("Route:!justShapeAdded: " + !justAddedShape.Contains(serviceManager.Get<IFlowSharpCanvasService>().ActiveController.GetRootShapeAt(CurrentMousePosition)));
                    Trace.WriteLine("Route:!DraggingOccurred: " + !DraggingOccurred);
                }
            });

            // Right-click on shape
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.StartDragSelectionBox,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsRootShapeSelectable(CurrentMousePosition) && CurrentButtons == MouseButtons.Right,
                Action = (_) =>
                {
                    RightClick();
                },
            });

            // SELECTION BOX

            // Start selection box
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.StartDragSelectionBox,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => !serviceManager.Get<IFlowSharpCanvasService>().ActiveController.IsRootShapeSelectable(CurrentMousePosition) && CurrentButtons == MouseButtons.Right,
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

            // Edit Text

            router.Add(new MouseRouter()
            {
                RouteName = RouteName.EditShapeText,
                MouseEvent = MouseEvent.MouseDoubleClick,
                Condition = () => true,
                Action = (_) =>
                {
                    if (doubleClickCounter == 0)
                    {
                        ++doubleClickCounter;
                        serviceManager.IfExists<IFlowSharpEditService>(svc => svc.EditText());

                        // Reset counter after 1/2 second, so the second double click is ignored.
                        Task.Run(() =>
                        {
                            Thread.Sleep(500);
                            doubleClickCounter = 0;
                        });
                    }
                }
            });
        }

        protected bool MouseHasReallyMoved()
        {
            return CurrentMousePosition.Delta(LastMousePosition) != Point.Empty;
        }

        protected virtual void HandleEvent(MouseAction action)
        {
            Trace.WriteLine("Route:HandleEvent:" + CurrentButtons.ToString());
            CurrentMousePosition = action.MousePosition;
            CurrentButtons = Control.MouseButtons;
            // Issue #39: Mouse Move event fires even for button press when mouse hasn't moved!
            IEnumerable<MouseRouter> routes = router.Where(r => (action.MouseEvent != MouseEvent.MouseMove && r.MouseEvent == action.MouseEvent)
                || ((action.MouseEvent == MouseEvent.MouseMove && r.MouseEvent == action.MouseEvent && MouseHasReallyMoved())));

            routes.ForEach(r =>
            {
                Trace.WriteLine("Route:Executing Route:" + r.RouteName.ToString());
                r.Debug?.Invoke();

                // Test condition every time after executing a route handler, as the handler may change state for the next condition.
                if (r.Condition())
                {
                    Trace.WriteLine("Route:Executing Route:" + r.RouteName.ToString());
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
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            Point delta = CurrentMousePosition.Delta(LastMousePosition);
            controller.Canvas.Cursor = Cursors.SizeAll;
            // Pick up every object on the canvas and move it.
            // This does not "move" the grid.
            controller.MoveAllElements(delta);

            // Conversely, we redraw the grid and invalidate, which forces all the elements to redraw.
            //canvas.Drag(delta);
            //elements.ForEach(el => el.Move(delta));
            //canvas.Invalidate();
        }

        protected void ShowAnchors()
        {
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            GraphicElement el = controller.GetRootShapeAt(CurrentMousePosition);
            Trace.WriteLine("*** ShowAnchors " + el.GetType().Name);
            el.ShowAnchors = true;
            controller.Redraw(el);
            HoverShape = el;
            controller.SetAnchorCursor(el);
        }

        protected void ChangeAnchors()
        {
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            HoverShape.ShowAnchors = false;
            controller.Redraw(HoverShape);
            HoverShape = controller.GetRootShapeAt(CurrentMousePosition);
            HoverShape.ShowAnchors = true;
            controller.Redraw(HoverShape);
            controller.SetAnchorCursor(HoverShape);
        }

        protected void HideAnchors()
        {
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            HoverShape.ShowAnchors = false;
            controller.Redraw(HoverShape);
            controller.Canvas.Cursor = Cursors.Arrow;
            HoverShape = null;
        }

        protected void SelectSingleRootShape()
        {
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            // Preserve for undo:
            List<GraphicElement> selectedShapes = controller.SelectedElements.ToList();
            GraphicElement el = controller.GetRootShapeAt(CurrentMousePosition);

            if (selectedShapes.Count != 1 || !selectedShapes.Contains(el))
            {
                controller.UndoStack.UndoRedo("Select Root " + el.ToString(),
                    () =>
                    {
                        controller.DeselectCurrentSelectedElements();
                        controller.SelectElement(el);
                    },
                    () =>
                    {
                        controller.DeselectCurrentSelectedElements();
                        controller.SelectElements(selectedShapes);
                    });
            }
        }

        protected void SelectSingleChildShape()
        {
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            // Preserve for undo:
            List<GraphicElement> selectedShapes = controller.SelectedElements.ToList();
            GraphicElement el = controller.GetChildShapeAt(CurrentMousePosition);
            controller.UndoStack.UndoRedo("Select Child " + el.ToString(),
                () =>
                {
                    controller.DeselectCurrentSelectedElements();
                    controller.SelectElement(el);
                },
                () =>
                {
                    controller.DeselectCurrentSelectedElements();
                    controller.SelectElements(selectedShapes);
                });
        }

        protected void AddShapeToSelectionList()
        {
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            // Preserve for undo:
            List<GraphicElement> selectedShapes = controller.SelectedElements.ToList();

            GraphicElement el = controller.GetRootShapeAt(CurrentMousePosition);
            controller.UndoStack.UndoRedo("Select " + el.ToString(),
                () =>
                {
                    controller.DeselectGroupedElements();
                    controller.SelectElement(el);
                    justAddedShape.Add(el);
                },
                () =>
                {
                    controller.DeselectCurrentSelectedElements();
                    controller.SelectElements(selectedShapes);
                });
        }

        protected void RemoveShapeFromSelectionList()
        {
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            GraphicElement el = controller.GetRootShapeAt(CurrentMousePosition);
            controller.UndoStack.UndoRedo("Deselect " + el.ToString(),
                () =>
                {
                    controller.DeselectElement(el);
                },
                () =>
                {
                    controller.SelectElement(el);
                });
        }

        protected void DragShapes()
        {
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            controller.Canvas.Cursor = Cursors.SizeAll;
            Point delta = CurrentMousePosition.Delta(LastMousePosition);

            if (controller.SelectedElements.Count == 1 && controller.SelectedElements[0].IsConnector)
            {
                // Check both ends of any connector being moved.
                if (!controller.SnapController.SnapCheck(GripType.Start, delta, (snapDelta) => controller.DragSelectedElements(snapDelta)))
                {
                    if (!controller.SnapController.SnapCheck(GripType.End, delta, (snapDelta) => controller.DragSelectedElements(snapDelta)))
                    {
                        controller.DragSelectedElements(delta);
                        controller.SnapController.UpdateRunningDelta(delta);
                    }
                }
            }
            else
            {
                controller.DragSelectedElements(delta);
                controller.SnapController.UpdateRunningDelta(delta);
            }
        }

        protected void ClearAnchorCursor()
        {
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            controller.Canvas.Cursor = Cursors.Arrow;
        }

        protected void SetAnchorCursor()
        {
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            ShapeAnchor anchor = HoverShape.GetAnchors().FirstOrDefault(a => a.Near(CurrentMousePosition));

            // Hover shape could have changed as we move from a shape to a connector's anchor.
            if (anchor != null)
            {
                controller.Canvas.Cursor = anchor.Cursor;
            }
        }

        protected void DragAnchor()
        {
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            Point delta = CurrentMousePosition.Delta(LastMousePosition);
            GraphicElement hoverShape = HoverShape;
            ShapeAnchor selectedAnchor = SelectedAnchor;

            if (!controller.SnapController.SnapCheck(selectedAnchor.Type, delta, (snapDelta) => hoverShape.UpdateSize(selectedAnchor, snapDelta)))
            {
                hoverShape.UpdateSize(selectedAnchor, delta);
                controller.SnapController.UpdateRunningDelta(delta);
            }
        }

        protected void RightClick()
        {
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            GraphicElement hoverShape = HoverShape;

            // Sometimes this is null.  Not sure why.
            hoverShape?.RightClick();
        }

        protected void CreateSelectionBox()
        {
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            SelectionBox = new Box(controller.Canvas);
            SelectionBox.BorderPen.Color = Color.Gray;
            SelectionBox.FillBrush.Color = Color.Transparent;
            SelectionBox.DisplayRectangle = new Rectangle(StartSelectionPosition, new Size(1, 1));
            controller.Insert(SelectionBox);
        }

        protected void SelectShapesInSelectionBox()
        {
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            controller.DeleteElement(SelectionBox);
            List<GraphicElement> selectedElements = new List<GraphicElement>();
            List<GraphicElement> previouslySelectedElements = controller.SelectedElements.ToList();

            controller.Elements.Where(e => !selectedElements.Contains(e) && e.Parent == null && SelectionBox.DisplayRectangle.Contains(e.UpdateRectangle)).ForEach((e) =>
            {
                selectedElements.Add(e);
            });

            controller.UndoStack.UndoRedo("Group Select",
                () =>
                {
                    controller.DeselectCurrentSelectedElements();
                    controller.SelectElements(selectedElements);
                },
                () =>
                {
                    controller.DeselectCurrentSelectedElements();
                    controller.SelectElements(previouslySelectedElements);
                });
            // Why was this here?
            // Controller.Canvas.Invalidate();
        }

        protected void DragSelectionBox()
        {
            BaseController controller = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            // Normalize the rectangle to a top-left, bottom-right rectangle.
            int x = CurrentMousePosition.X.Min(StartSelectionPosition.X);
            int y = CurrentMousePosition.Y.Min(StartSelectionPosition.Y);
            int w = (CurrentMousePosition.X - StartSelectionPosition.X).Abs();
            int h = (CurrentMousePosition.Y - StartSelectionPosition.Y).Abs();
            Rectangle newRect = new Rectangle(x, y, w, h);
            Point delta = CurrentMousePosition.Delta(LastMousePosition);
            controller.UpdateDisplayRectangle(SelectionBox, newRect, delta);
        }
    }
}
