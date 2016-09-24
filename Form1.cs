using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FlowSharp
{
    public partial class Form1 : Form
    {
		public const int MIN_WIDTH = 20;
		public const int MIN_HEIGHT = 20;

        protected List<GraphicElement> elements = new List<GraphicElement>();
        protected bool dragging;
        protected bool leftMouseDown;
        protected GraphicElement selectedElement;
		protected Anchor selectedAnchor;
		protected GraphicElement showingAnchorsElement;
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
			elements.Add(new Diamond() { DisplayRectangle = new Rectangle(325, 100, 40, 40) });

			elements.ForEach(el => el.UpdatePath());

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
				selectedAnchor = null;
                leftMouseDown = false;
                // DeselectCurrentSelectedElement(gr);
                dragging = !(args.Button == MouseButtons.Left);
            };

            surface.MouseMove += OnMouseMove;
        }

		private void OnMouseMove(object sender, MouseEventArgs args)
		{
			Point delta = args.Location.Delta(mousePosition);
			mousePosition = args.Location;

			if (dragging)
			{
				if (selectedAnchor == null)
				{
					selectedAnchor = selectedElement.GetAnchors().FirstOrDefault(a => a.Near(mousePosition));
				}

				if (selectedAnchor != null)
				{
					UpdateSize(selectedElement, selectedAnchor, delta);
				}
				else
				{
					MoveElement(selectedElement, delta);
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
				//surface.Drag(delta);
				//elements.ForEach(el => el.Move(delta));
				//surface.Invalidate();
			}
			else
			{
				GraphicElement el = elements.FirstOrDefault(e => e.DisplayRectangle.Contains(args.Location));

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

        protected void MoveElement(GraphicElement el, Point delta)
        {
            if (surface.OnScreen(el.UpdateRectangle))
            {
                List<GraphicElement> els = EraseTopToBottom(el, delta.X.Abs(), delta.Y.Abs());
                el.Move(delta);
				el.UpdatePath();
                DrawBottomToTop(els, delta.X.Abs(), delta.Y.Abs());
            }
            else
            {
                el.CancelBackground();
                el.Move(delta);
            }
        }

		protected void UpdateSize(GraphicElement el, Anchor anchor, Point delta)
		{
			Point adjustedDelta = anchor.AdjustedDelta(delta);
			Rectangle newRect = anchor.Resize(el.DisplayRectangle, adjustedDelta);

			// Don't get too small.
			if (newRect.Width > MIN_WIDTH && newRect.Height > MIN_HEIGHT)
			{
				List<GraphicElement> els = EraseTopToBottom(el, adjustedDelta.X.Abs(), adjustedDelta.Y.Abs());
				el.DisplayRectangle = newRect;
				el.UpdatePath();
				DrawBottomToTop(els, adjustedDelta.X.Abs(), adjustedDelta.Y.Abs());
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

		/// <summary>
		/// Recursive loop to get all intersecting rectangles, including intersectors of the intersectees, so that all elements that
		/// are affected by an overlap redraw are erased and redrawn, otherwise we get artifacts of some intersecting elements when intersection count > 2.
		/// </summary>
        protected void FindAllIntersections(List<GraphicElement> intersections, GraphicElement el, int dx = 0, int dy = 0)
        {
			// Cool thing here is that if the element has no intersections, this list still returns that element because it intersects with itself!
            elements.Where(e => !intersections.Contains(e) && e.UpdateRectangle.IntersectsWith(el.UpdateRectangle.Grow(dx, dy))).ForEach((e) =>
            {
                intersections.Add(e);
                FindAllIntersections(intersections, e);
            });
        }

		protected void Redraw(GraphicElement el)
		{
			var els = EraseTopToBottom(el);
			DrawBottomToTop(els);
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
