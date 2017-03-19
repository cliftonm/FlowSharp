/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Drawing;

using FlowSharpCodeShapeInterfaces;
using FlowSharpLib;

namespace FlowSharpCodeDrakonShapes
{
    [ExcludeFromToolbox]
    public class OutputBox : GraphicElement, IDrakonShape, IOutputBox
    {
        protected Point[] path;
        protected const int INDENT_SIZE = 12;

        public OutputBox(Canvas canvas) : base(canvas)
        {
            HasCornerConnections = false;
        }

        public override void UpdatePath()
        {
            path = new Point[]
            {
                new Point(ZoomRectangle.X, ZoomRectangle.Y),                                                                           // top left
                new Point(ZoomRectangle.X + ZoomRectangle.Width - INDENT_SIZE,    ZoomRectangle.Y),                                // top right of indented right "arrow"
                new Point(ZoomRectangle.X + ZoomRectangle.Width, ZoomRectangle.Y + ZoomRectangle.Height/2),                     // right tip (middle of box)
                new Point(ZoomRectangle.X + ZoomRectangle.Width - INDENT_SIZE, ZoomRectangle.Y + ZoomRectangle.Height),         // bottom right of indented right "arrow"
                new Point(ZoomRectangle.X, ZoomRectangle.Y + ZoomRectangle.Height),                                              // bottom left 
                new Point(ZoomRectangle.X, ZoomRectangle.Y + ZoomRectangle.Height/2),                                             // middle left of indented left "arrow"
            };
        }

        public override void Draw(Graphics gr, bool showSelection = true)
        {
            gr.FillPolygon(FillBrush, path);
            gr.DrawPolygon(BorderPen, path);
            base.Draw(gr, showSelection);
        }
    }

    [ToolboxShape]
    [ToolboxOrder(4)]
    public class ToolboxOutputBox : GraphicElement
    {
        protected Point[] path;
        protected const int INDENT_SIZE = 5;
        protected const int V_ADJ = 4;

        public ToolboxOutputBox(Canvas canvas) : base(canvas)
        {
            HasCornerConnections = false;
        }

        public override GraphicElement CloneDefault(Canvas canvas)
        {
            return CloneDefault(canvas, Point.Empty);
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
            OutputBox shape = new OutputBox(canvas);
            shape.DisplayRectangle = shape.DefaultRectangle().Move(offset);
            shape.UpdateProperties();
            shape.UpdatePath();

            return shape;
        }

        public override void UpdatePath()
        {
            path = new Point[]
            {
                new Point(DisplayRectangle.X, DisplayRectangle.Y + V_ADJ),                                                                  // top left 
                new Point(DisplayRectangle.X + DisplayRectangle.Width - INDENT_SIZE, DisplayRectangle.Y + V_ADJ),                                         // top right of indented right "arrow"
                new Point(DisplayRectangle.X + DisplayRectangle.Width, DisplayRectangle.Y + DisplayRectangle.Height/2),                                   // right tip (middle of box)
                new Point(DisplayRectangle.X + DisplayRectangle.Width - INDENT_SIZE, DisplayRectangle.Y + DisplayRectangle.Height - V_ADJ),               // bottom right of indented right "arrow"
                new Point(DisplayRectangle.X, DisplayRectangle.Y + DisplayRectangle.Height - V_ADJ),                                        // bottom left 
                new Point(DisplayRectangle.X, DisplayRectangle.Y + DisplayRectangle.Height/2),                                                            // middle left of indented left "arrow"
            };
        }

        public override void Draw(Graphics gr, bool showSelection = true)
        {
            gr.FillPolygon(FillBrush, path);
            gr.DrawPolygon(BorderPen, path);
            base.Draw(gr, showSelection);
        }
    }
}
