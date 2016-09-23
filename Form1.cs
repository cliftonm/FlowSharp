using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FlowSharp
{
    public partial class Form1 : Form
    {
        protected List<GraphicElement> elements = new List<GraphicElement>();
        protected bool dragging;
        protected bool leftMouseDown;
        protected GraphicElement selectedElement;
        protected Point mousePosition;
        protected Surface surface;

        public Form1()
        {
            InitializeComponent();
            Shown += OnShown;
        }

        public void OnShown(object sender, EventArgs e)
        { 
            surface = Surface.Initialize(this, SurfacePaintComplete);
            Graphics gr = surface.Graphics;
            elements.Add(new Box() { DisplayRectangle = new Rectangle(25, 50, 200, 25) });
            elements.Add(new Box() { DisplayRectangle = new Rectangle(225, 250, 100, 25) });
            elements.Add(new Ellipse() { DisplayRectangle = new Rectangle(125, 100, 100, 75) });
            surface.MouseDown += (sndr, args) =>
            {
                leftMouseDown = true;
                DeselectCurrentSelectedElement(gr);
                SelectElement(gr, args.Location);
                dragging = args.Button == MouseButtons.Left && selectedElement != null;
                mousePosition = args.Location;
            };

            surface.MouseUp += (sndr, args) =>
            {
                leftMouseDown = false;
                // DeselectCurrentSelectedElement(gr);
                dragging = !(args.Button == MouseButtons.Left);
            };

            surface.MouseMove += OnMouseMove;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            Point delta = e.Location.Delta(mousePosition);
            mousePosition = e.Location;

            if (dragging)
            {
                MoveElement(selectedElement, delta);
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
                //surface.Drag(delta);
                //elements.ForEach(el => el.Move(delta));
                //surface.Invalidate();
            }
        }

        protected void MoveElement(GraphicElement el, Point delta)
        {
            if (surface.OnScreen(el.UpdateRectangle))
            {
                Rectangle rectBeforeMove = el.UpdateRectangle;

                List<GraphicElement> els = EraseTopToBottom(el, delta.X.Abs(), delta.Y.Abs());
                el.Move(delta);
                DrawBottomToTop(els, delta.X.Abs(), delta.Y.Abs());
            }
            else
            {
                el.CancelBackground();
                el.Move(delta);
            }
        }

        protected void DeselectCurrentSelectedElement(Graphics gr)
        {
            if (selectedElement != null)
            {
                var els = EraseTopToBottom(selectedElement);
                selectedElement.Selected = false;
                DrawBottomToTop(els);
                selectedElement = null;
            }
        }

        protected bool SelectElement(Graphics gr, Point p)
        {
            GraphicElement el = elements.FirstOrDefault(e => e.DisplayRectangle.Contains(p));

            if (el != null)
            {
                var els = EraseTopToBottom(el);
                el.Selected = true;
                DrawBottomToTop(els);
            }

            selectedElement = el;

            return el != null;
        }

        protected void FindAllIntersections(List<GraphicElement> intersections, GraphicElement el, int dx = 0, int dy = 0)
        {
            elements.Where(e => !intersections.Contains(e) && e.UpdateRectangle.IntersectsWith(el.UpdateRectangle.Grow(dx, dy))).ForEach((e) =>
            {
                intersections.Add(e);
                FindAllIntersections(intersections, e);
            });
        }

        protected List<GraphicElement> EraseTopToBottom(GraphicElement el, int dx=0, int dy=0)
        {
            List<GraphicElement> intersections = new List<GraphicElement>();
            FindAllIntersections(intersections, el, dx, dy);
            List<GraphicElement> els = intersections.OrderBy(e => elements.IndexOf(e)).ToList();
            els.Where(e => surface.OnScreen(e.UpdateRectangle.Grow(dx, dy))).ForEach(e => e.Erase(surface));

            return els;
        }

        protected void DrawBottomToTop(List<GraphicElement> els, int dx=0, int dy=0)
        {
			// Don't modify the original list.
			els.AsEnumerable().Reverse().Where(e=>surface.OnScreen(e.UpdateRectangle.Grow(dx, dy))).ForEach(e =>
            {
                e.GetBackground(surface);
                e.Draw(surface);
            });

            // Is this faster than creating a unioned rectangle?  Dunno, because the unioned rectangle might include a lot of space not part of the shapes, like something in an "L" pattern.
            els.Where(e => surface.OnScreen(e.UpdateRectangle.Grow(dx, dy))).ForEach(e => e.UpdateScreen(surface, dx, dy));
        }

        protected void SurfacePaintComplete(Surface surface)
        {
			DrawBottomToTop(elements);
        }
    }
}
