/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Drawing;

using FlowSharpLib;

namespace FlowSharpCodeShapes
{
    [ExcludeFromToolbox]
    public class AgentShape : GraphicElement
    {
        protected Point[] path;

        public AgentShape(Canvas canvas) : base(canvas)
        {
            Text = "Agent";
            FillBrush.Color = Color.PowderBlue;
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
            GraphicElement el = base.CloneDefault(canvas, offset);
            el.Text = "Agent";
            FillBrush.Color = Color.PowderBlue;

            return el;
        }

        public override void UpdatePath()
        {
            int indentSize = ZoomRectangle.Width / 3;
            path = new Point[]
            {
                new Point(ZoomRectangle.X + indentSize, ZoomRectangle.Y),                                                            // top left of indented left "arrow"
                new Point(ZoomRectangle.X + ZoomRectangle.Width - indentSize,    ZoomRectangle.Y),                                // top right of indented right "arrow"
                new Point(ZoomRectangle.X + ZoomRectangle.Width, ZoomRectangle.Y + ZoomRectangle.Height/2),                     // right tip (middle of box)
                new Point(ZoomRectangle.X + ZoomRectangle.Width - indentSize, ZoomRectangle.Y + ZoomRectangle.Height),         // bottom right of indented right "arrow"
                new Point(ZoomRectangle.X + indentSize, ZoomRectangle.Y + ZoomRectangle.Height),                                  // bottom left of indented left "arrow"
                new Point(ZoomRectangle.X, ZoomRectangle.Y + ZoomRectangle.Height/2),                                                            // middle left of indented left "arrow"
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
    [ToolboxOrder(9)]
    public class ToolboxAssemblyAgentShape : AgentShape
    {
        public override Rectangle ToolboxDisplayRectangle { get { return new Rectangle(0, 0, 35, 25); } }

        public ToolboxAssemblyAgentShape(Canvas canvas) : base(canvas)
        {
            Text = "Agent";
            TextFont.Dispose();
            TextFont = new Font(FontFamily.GenericSansSerif, 6);
            FillBrush.Color = Color.PowderBlue;
        }

        public override GraphicElement CloneDefault(Canvas canvas)
        {
            return CloneDefault(canvas, Point.Empty);
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
            AgentShape shape = new AgentShape(canvas);
            shape.DisplayRectangle = shape.DefaultRectangle().Move(offset);
            shape.UpdateProperties();
            shape.UpdatePath();

            return shape;
        }
    }
}