/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Drawing;

namespace FlowSharpLib
{
    [ExcludeFromToolbox]
    public class EndForLoopBox : GraphicElement
    {
        protected Point[] path;
        protected const int INDENT_SIZE = 14;
        protected const int Y_ADJUST = 5;

        public EndForLoopBox(Canvas canvas) : base(canvas)
        {
            HasCornerConnections = false;
        }

        public override void UpdatePath()
        {
            path = new Point[]
            {
                new Point(DisplayRectangle.X, DisplayRectangle.Y + Y_ADJUST),                                                            // top left of indented left "arrow"
                new Point(DisplayRectangle.X + DisplayRectangle.Width,    DisplayRectangle.Y + Y_ADJUST),                                // top right of indented right "arrow"
                new Point(DisplayRectangle.X + DisplayRectangle.Width, DisplayRectangle.Y + DisplayRectangle.Height/2),                     // right tip (middle of box)
                new Point(DisplayRectangle.X + DisplayRectangle.Width - INDENT_SIZE, DisplayRectangle.Y + DisplayRectangle.Height - Y_ADJUST),         // bottom right of indented right "arrow"
                new Point(DisplayRectangle.X + INDENT_SIZE, DisplayRectangle.Y + DisplayRectangle.Height -  Y_ADJUST),                                  // bottom left of indented left "arrow"
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

    [ToolboxShape]
    [ToolboxOrder(4)]
    public class ToolboxEndForLoopBox : GraphicElement
    {
        protected Point[] path;
        protected const int INDENT_SIZE = 6;
        protected const int Y_ADJUST = 5;

        public ToolboxEndForLoopBox(Canvas canvas) : base(canvas)
        {
            HasCornerConnections = false;
        }

        public override GraphicElement CloneDefault(Canvas canvas)
        {
            return CloneDefault(canvas, Point.Empty);
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
            EndForLoopBox shape = new EndForLoopBox(canvas);
            shape.DisplayRectangle = shape.DefaultRectangle().Move(offset);
            shape.UpdateProperties();
            shape.UpdatePath();

            return shape;
        }

        public override void UpdatePath()
        {
            path = new Point[]
            {
                new Point(DisplayRectangle.X, DisplayRectangle.Y + Y_ADJUST),                                                            // top left of indented left "arrow"
                new Point(DisplayRectangle.X + DisplayRectangle.Width,    DisplayRectangle.Y + Y_ADJUST),                                // top right of indented right "arrow"
                new Point(DisplayRectangle.X + DisplayRectangle.Width, DisplayRectangle.Y + DisplayRectangle.Height/2),                     // right tip (middle of box)
                new Point(DisplayRectangle.X + DisplayRectangle.Width - INDENT_SIZE, DisplayRectangle.Y + DisplayRectangle.Height - Y_ADJUST),         // bottom right of indented right "arrow"
                new Point(DisplayRectangle.X + INDENT_SIZE, DisplayRectangle.Y + DisplayRectangle.Height -  Y_ADJUST),                                  // bottom left of indented left "arrow"
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
