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
    public class InputBox : GraphicElement, IDrakonShape, IInputBox
    {
        protected Point[] path;
        protected const int INDENT_SIZE = 12;

        public InputBox(Canvas canvas) : base(canvas)
        {
            HasCornerConnections = false;
        }

        public override void UpdatePath()
        {
            path = new Point[]
            {
                new Point(ZoomRectangle.X + INDENT_SIZE, ZoomRectangle.Y),                                           // top left of indented left "arrow"
                new Point(ZoomRectangle.X + ZoomRectangle.Width, ZoomRectangle.Y),                                // top right of indented right "arrow"
                new Point(ZoomRectangle.X + ZoomRectangle.Width, ZoomRectangle.Y + ZoomRectangle.Height/2),    // right tip (middle of box)
                new Point(ZoomRectangle.X + ZoomRectangle.Width, ZoomRectangle.Y + ZoomRectangle.Height),      // bottom right of indented right "arrow"
                new Point(ZoomRectangle.X + INDENT_SIZE, ZoomRectangle.Y + ZoomRectangle.Height),                 // bottom left of indented left "arrow"
                new Point(ZoomRectangle.X, ZoomRectangle.Y + ZoomRectangle.Height/2),                             // middle left of indented left "arrow"
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
    public class ToolboxInputBox : GraphicElement
    {
        protected Point[] path;
        protected const int INDENT_SIZE = 5;
        protected const int V_ADJ = 4;

        public ToolboxInputBox(Canvas canvas) : base(canvas)
        {
            HasCornerConnections = false;
        }

        public override GraphicElement CloneDefault(Canvas canvas)
        {
            return CloneDefault(canvas, Point.Empty);
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
            InputBox shape = new InputBox(canvas);
            shape.DisplayRectangle = shape.DefaultRectangle().Move(offset);
            shape.UpdateProperties();
            shape.UpdatePath();

            return shape;
        }

        public override void UpdatePath()
        {
            path = new Point[]
            {
                new Point(DisplayRectangle.X + INDENT_SIZE, DisplayRectangle.Y + V_ADJ),                                                    // top left of indented left "arrow"
                new Point(DisplayRectangle.X + DisplayRectangle.Width, DisplayRectangle.Y + V_ADJ),                                         // top right
                new Point(DisplayRectangle.X + DisplayRectangle.Width, DisplayRectangle.Y + DisplayRectangle.Height/2),                     // right tip (middle of box)
                new Point(DisplayRectangle.X + DisplayRectangle.Width, DisplayRectangle.Y + DisplayRectangle.Height - V_ADJ),               // bottom right
                new Point(DisplayRectangle.X + INDENT_SIZE, DisplayRectangle.Y + DisplayRectangle.Height - V_ADJ),                          // bottom left of indented left "arrow"
                new Point(DisplayRectangle.X, DisplayRectangle.Y + DisplayRectangle.Height/2),                                              // middle left of indented left "arrow"
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
