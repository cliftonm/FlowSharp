using System;
using System.Collections.Generic;
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

        public MouseAction(MouseController.MouseEvent mouseEvent, Point mousePosition, MouseButtons buttons)
        {
            MouseEvent = mouseEvent;
            MousePosition = mousePosition;
            Buttons = buttons;
        }
    }

    public class MouseRouter
    {
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
        public bool SelectingShapes { get; set; }
        public GraphicElement HoverShape { get; set; }

        public BaseController Controller { get; protected set; }

        protected List<MouseRouter> router;

        public enum MouseEvent
        {
            MouseDown,
            MouseUp,
            MouseMove,
        }

        public MouseController(BaseController controller)
        {
            Controller = controller;
            router = new List<MouseRouter>();
        }

        public void HookMouseEvents()
        {
            Controller.Canvas.MouseDown += (sndr, args) => HandleEvent(new MouseAction(MouseEvent.MouseDown, args.Location, args.Button));
            Controller.Canvas.MouseUp += (sndr, args) => HandleEvent(new MouseAction(MouseEvent.MouseUp, args.Location, args.Button));
            Controller.Canvas.MouseMove += (sndr, args) => HandleEvent(new MouseAction(MouseEvent.MouseMove, args.Location, args.Button));
        }

        public virtual void InitializeBehavior()
        {
            // DRAG SURFACE ROUTES:

            // Start drag surface:
            router.Add(new MouseRouter()
            {
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => !Controller.IsShapeSelectable(CurrentMousePosition) && CurrentButtons == MouseButtons.Left,
                Action = () => DraggingSurface = true
            });

            // End drag surface:
            router.Add(new MouseRouter()
            {
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => DraggingSurface,
                Action = () =>
                {
                    DraggingSurface = false;
                    Controller.Canvas.Cursor = Cursors.Arrow;
                }
            });

            // Drag surface:
            router.Add(new MouseRouter()
            {
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => DraggingSurface,
                Action = () => DragCanvas(),
            });

            // SHAPE DRAGGING ROUTES:

            // Start shape drag:
            router.Add(new MouseRouter()
            {
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => Controller.IsShapeSelectable(CurrentMousePosition) && CurrentButtons == MouseButtons.Left,
                Action = () => DraggingShapes = true
            });

            // End shape dragging:
            router.Add(new MouseRouter()
            {
                MouseEvent = MouseEvent.MouseUp,
                Condition = () => DraggingShapes,
                Action = () =>
                {
                    DraggingShapes = false;
                    Controller.Canvas.Cursor = Cursors.Arrow;
                }
            });

            // Drag shapes:
            router.Add(new MouseRouter()
            {
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => DraggingShapes,
                Action = () => DragShapes(),
            });

            // HOVER ROUTES

            // Show anchors when hovering over a shape
            router.Add(new MouseRouter()
            {
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => !DraggingSurface && !DraggingShapes && !SelectingShapes && HoverShape == null && 
                    Controller.IsShapeSelectable(CurrentMousePosition),
                Action = () => ShowAnchors(),
            });

            // Change anchors when hover shape changes
            router.Add(new MouseRouter()
            {
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => !DraggingSurface && !DraggingShapes && !SelectingShapes && HoverShape != null &&
                    Controller.IsShapeSelectable(CurrentMousePosition) &&
                    HoverShape != Controller.GetShapeAt(CurrentMousePosition),
                Action = () => ChangeAnchors(),
            });

            // Hide anchors when not hovering over a shape
            router.Add(new MouseRouter()
            {
                MouseEvent = MouseEvent.MouseMove,
                Condition = () => !DraggingSurface && !DraggingShapes && !SelectingShapes && HoverShape != null &&
                    !Controller.IsShapeSelectable(CurrentMousePosition),
                Action = () => HideAnchors(),
            });

            // SHAPE SELECTION

            // Select a shape
            router.Add(new MouseRouter()
            {
                MouseEvent = MouseEvent.MouseDown,
                Condition = () => Controller.IsShapeSelectable(CurrentMousePosition),
                Action = () => SelectShape()
            });
        }

        protected virtual void HandleEvent(MouseAction action)
        {
            CurrentMousePosition = action.MousePosition;
            CurrentButtons = action.Buttons;

            IEnumerable<MouseRouter> routes = router.Where(r => r.MouseEvent == action.MouseEvent && r.Condition());
            routes.ForEach(r => r.Action());

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

        protected void SelectShape()
        {
            if (!Controller.IsMultiSelect())
            {
                Controller.DeselectCurrentSelectedElements();
            }

            GraphicElement el = Controller.GetShapeAt(CurrentMousePosition);
            Controller.SelectElement(el);
        }

        protected void DragShapes()
        {
            Point delta = CurrentMousePosition.Delta(LastMousePosition);
            Controller.DragSelectedElements(delta);
            Controller.Canvas.Cursor = Cursors.SizeAll;
        }
    }
}
