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

        public MouseAction(MouseController.MouseEvent mouseEvent, Point mousePosition)
        {
            MouseEvent = mouseEvent;
            MousePosition = mousePosition;
            // Buttons = buttons;
        }
    }

    public class MouseRouter
    {
        public MouseController.RouteName RouteName { get; set; }
        public MouseController.MouseEvent MouseEvent { get; set; }
        public Func<bool> Condition { get; set; }
        public Action Action { get; set; }
    }

    public class MouseController
    {
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

        public enum MouseEvent
        {
            MouseDown,
            MouseUp,
            MouseMove,
        }

        public enum RouteName
        {
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
            DragShapes,
            DragAnchor,
            HoverOverShape,
            ShowAnchors,
            ShowAnchorCursor,
            ClearAnchorCursor,
            HideAnchors,
            SelectSingleShape,
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
            Controller.Canvas.MouseDown += (sndr, args) => HandleEvent(new MouseAction(MouseEvent.MouseDown, args.Location));
            Controller.Canvas.MouseUp += (sndr, args) => HandleEvent(new MouseAction(MouseEvent.MouseUp, args.Location));
            Controller.Canvas.MouseMove += (sndr, args) => HandleEvent(new MouseAction(MouseEvent.MouseMove, args.Location));
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
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.CanvasFocus,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => true,
                Action = () =>
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
                Condition = () => !Controller.IsShapeSelectable(CurrentMousePosition) && CurrentButtons == MouseButtons.Left,
                Action = () =>
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
                Action = () =>
                {
                    Controller.DeselectCurrentSelectedElements();
                    DraggingSurface = false;
                    Controller.Canvas.Cursor = Cursors.Arrow;
                }
            });

            // End drag surface when dragging occurred, selected shapes stay selected.
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.EndDragSurface,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => DraggingSurface && DraggingSurfaceOccurred,
                Action = () =>
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
                Action = () =>
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
                Condition = () => Controller.IsShapeSelectable(CurrentMousePosition) &&
                    CurrentButtons == MouseButtons.Left &&
                    Controller.GetShapeAt(CurrentMousePosition).GetAnchors().FirstOrDefault(a => a.Near(CurrentMousePosition)) == null,
                Action = () => DraggingShapes = true
            });

            // Start anchor drag:
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.StartShapeDrag,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => Controller.IsShapeSelectable(CurrentMousePosition) &&
                    CurrentButtons == MouseButtons.Left &&
                    Controller.GetShapeAt(CurrentMousePosition).GetAnchors().FirstOrDefault(a => a.Near(CurrentMousePosition)) != null,
                Action = () =>
                {
                    DraggingAnchor = true;
                    SelectedAnchor = HoverShape.GetAnchors().First(a => a.Near(CurrentMousePosition));
                },
            });

            // End shape/anchor dragging:
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.EndShapeDrag,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => DraggingShapes || DraggingAnchor,
                Action = () =>
                {
                    Controller.HideConnectionPoints();
                    DraggingShapes = false;
                    DraggingOccurred = false;
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
                Condition = () => DraggingShapes && HoverShape.GetAnchors().FirstOrDefault(a => a.Near(CurrentMousePosition)) == null,
                Action = () =>
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
                Action = () =>
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
                    Controller.IsShapeSelectable(CurrentMousePosition) &&
                    Controller.GetShapeAt(CurrentMousePosition).Parent == null, // no anchors for grouped children.
                Action = () => ShowAnchors(),
            });

            // Change anchors when hover shape changes
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.ShowAnchors,
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => !DraggingSurface && !DraggingShapes && !SelectingShapes && HoverShape != null &&
                    CurrentButtons == MouseButtons.None &&
                    Controller.IsShapeSelectable(CurrentMousePosition) &&
                    HoverShape != Controller.GetShapeAt(CurrentMousePosition) &&
                    Controller.GetShapeAt(CurrentMousePosition).Parent == null, // no anchors for grouped children.
                Action = () => ChangeAnchors(),
            });

            // Hide anchors when not hovering over a shape
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.HideAnchors,
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => !DraggingSurface && !DraggingShapes && !SelectingShapes && HoverShape != null &&
                    CurrentButtons == MouseButtons.None &&
                    !Controller.IsShapeSelectable(CurrentMousePosition),
                Action = () => HideAnchors(),
            });

            // Show cursor when hovering over an anchor
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.ShowAnchorCursor,
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => !DraggingSurface && !DraggingShapes && !SelectingShapes && !DraggingAnchor && HoverShape != null &&
                    HoverShape.GetAnchors().FirstOrDefault(a => a.Near(CurrentMousePosition)) != null,
                Action = () => SetAnchorCursor(),
            });

            // Clear cursor when hovering over an anchor
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.ClearAnchorCursor,
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => !DraggingSurface && !DraggingShapes && !SelectingShapes && HoverShape != null &&
                    HoverShape.GetAnchors().FirstOrDefault(a => a.Near(CurrentMousePosition)) == null,
                Action = () => ClearAnchorCursor(),
            });

            // SHAPE SELECTION

            // Select a shape
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.SelectSingleShape,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => Controller.IsShapeSelectable(CurrentMousePosition) &&
                    !Controller.IsMultiSelect() &&
                    !Controller.SelectedElements.Contains(Controller.GetShapeAt(CurrentMousePosition)),
                Action = () => SelectSingleShape()
            });

            // Select a single shape
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.SelectSingleShape,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => Controller.IsShapeSelectable(CurrentMousePosition) &&
                    !Controller.IsMultiSelect() &&
                    !DraggingOccurred && !DraggingSelectionBox,
                Action = () => SelectSingleShape()
            });

            // Add another shape to selection list
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.AddSelectedShape,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => Controller.IsShapeSelectable(CurrentMousePosition) &&
                    Controller.IsMultiSelect() && !DraggingSelectionBox &&
                    !Controller.SelectedElements.Contains(Controller.GetShapeAt(CurrentMousePosition)),
                Action = () => AddShape(),
            });

            // Remove shape from selection list
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.RemoveSelectedShape,
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => Controller.IsShapeSelectable(CurrentMousePosition) &&
                    Controller.IsMultiSelect() && !DraggingSelectionBox &&
                    Controller.SelectedElements.Contains(Controller.GetShapeAt(CurrentMousePosition)) &&
                    !DraggingOccurred,
                Action = () => RemoveShape(),
            });

            // SELECTION BOX

            // Start selection box
            router.Add(new MouseRouter()
            {
                RouteName = RouteName.StartDragSelectionBox,
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => !Controller.IsShapeSelectable(CurrentMousePosition) && CurrentButtons == MouseButtons.Right,
                Action = () =>
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
                Action = () =>
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
                Action = () => DragSelectionBox(),
            });
        }

        protected virtual void HandleEvent(MouseAction action)
        {
            CurrentMousePosition = action.MousePosition;
            CurrentButtons = Control.MouseButtons;

            // Resolve now, otherwise the iterator will find additional routes as actions occur.
            // A good example is when a shape is added to a selection list, using the enumerator, this
            // then qualifies the remove shape from selected list!
            List<MouseRouter> routes = router.Where(r => r.MouseEvent == action.MouseEvent && r.Condition()).ToList();
            routes.ForEach(r =>
            {
                Trace.WriteLine("Route:" + r.RouteName.ToString());
                r.Action();
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
            GraphicElement el = Controller.GetShapeAt(CurrentMousePosition);
            el.ShowAnchors = true;
            Controller.Redraw(el);
            HoverShape = el;
            Controller.SetAnchorCursor(el);
        }

        protected void ChangeAnchors()
        {
            HoverShape.ShowAnchors = false;
            Controller.Redraw(HoverShape);
            HoverShape = Controller.GetShapeAt(CurrentMousePosition);
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

        protected void SelectSingleShape()
        {
            Controller.DeselectCurrentSelectedElements();
            GraphicElement el = Controller.GetShapeAt(CurrentMousePosition);
            Controller.SelectElement(el);
        }

        protected void AddShape()
        {
            GraphicElement el = Controller.GetShapeAt(CurrentMousePosition);
            Controller.SelectElement(el);
        }

        protected void RemoveShape()
        {
            GraphicElement el = Controller.GetShapeAt(CurrentMousePosition);
            Controller.DeselectElement(el);
        }

        protected void DragShapes()
        {
            Point delta = CurrentMousePosition.Delta(LastMousePosition);
            Controller.DragSelectedElements(delta);
            Controller.Canvas.Cursor = Cursors.SizeAll;
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
            bool connectorAttached = HoverShape.SnapCheck(SelectedAnchor, delta);

            if (!connectorAttached)
            {
                HoverShape.DisconnectShapeFromConnector(SelectedAnchor.Type);
                HoverShape.RemoveConnection(SelectedAnchor.Type);
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

            Controller.Elements.Where(e => !selectedElements.Contains(e) && e.Parent == null && e.UpdateRectangle.IntersectsWith(SelectionBox.DisplayRectangle)).ForEach((e) =>
            {
                selectedElements.Add(e);
            });

            Controller.DeselectCurrentSelectedElements();
            Controller.SelectElements(selectedElements);
            Controller.Canvas.Invalidate();
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
    }
}
