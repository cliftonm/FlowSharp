/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

using Clifton.Core.ExtensionMethods;

using FlowSharpCodeShapeInterfaces;
using FlowSharpLib;

namespace FlowSharpCodeDrakonShapes
{
    [ExcludeFromToolbox]
    public class AngleBracketBox : GraphicElement, IDrakonShape, IIfBox
    {
        public TruePath TruePath { get; set; }

        protected Point[] path;
        protected const int INDENT_SIZE = 12;

        public AngleBracketBox(Canvas canvas) : base(canvas)
        {
            HasCornerConnections = false;
            TruePath = TruePath.Down;           // Default path for the true condition.
        }

        public override ElementProperties CreateProperties()
        {
            return new AngleBracketBoxProperties(this);
        }

        public override void Serialize(ElementPropertyBag epb, IEnumerable<GraphicElement> elementsBeingSerialized)
        {
            Json["TruePath"] = TruePath.ToString();
            base.Serialize(epb, elementsBeingSerialized);
        }

        public override void Deserialize(ElementPropertyBag epb)
        {
            base.Deserialize(epb);

            string truePath;

            if (Json.TryGetValue("TruePath", out truePath))
            {
                TruePath = (TruePath)Enum.Parse(typeof(TruePath), truePath);
            }
        }

        public override void UpdatePath()
        {
            path = new Point[]
            {
                new Point(ZoomRectangle.X + INDENT_SIZE, ZoomRectangle.Y),                                                            // top left of indented left "arrow"
                new Point(ZoomRectangle.X + ZoomRectangle.Width - INDENT_SIZE,    ZoomRectangle.Y),                                // top right of indented right "arrow"
                new Point(ZoomRectangle.X + ZoomRectangle.Width, ZoomRectangle.Y + ZoomRectangle.Height/2),                     // right tip (middle of box)
                new Point(ZoomRectangle.X + ZoomRectangle.Width - INDENT_SIZE, ZoomRectangle.Y + ZoomRectangle.Height),         // bottom right of indented right "arrow"
                new Point(ZoomRectangle.X + INDENT_SIZE, ZoomRectangle.Y + ZoomRectangle.Height),                                  // bottom left of indented left "arrow"
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
    [ToolboxOrder(4)]
    public class ToolboxAngleBracketBox : GraphicElement
    {
        protected Point[] path;
        protected const int INDENT_SIZE = 5;
        protected const int V_ADJ = 3;

        public ToolboxAngleBracketBox(Canvas canvas) : base(canvas)
        {
            HasCornerConnections = false;
        }

        public override GraphicElement CloneDefault(Canvas canvas)
        {
            return CloneDefault(canvas, Point.Empty);
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
            AngleBracketBox shape = new AngleBracketBox(canvas);
            shape.DisplayRectangle = shape.DefaultRectangle().Move(offset);
            shape.UpdateProperties();
            shape.UpdatePath();

            return shape;
        }

        public override void UpdatePath()
        {
            path = new Point[]
            {
                new Point(DisplayRectangle.X + INDENT_SIZE, DisplayRectangle.Y + V_ADJ),                                                                  // top left of indented left "arrow"
                new Point(DisplayRectangle.X + DisplayRectangle.Width - INDENT_SIZE, DisplayRectangle.Y + V_ADJ),                                         // top right of indented right "arrow"
                new Point(DisplayRectangle.X + DisplayRectangle.Width, DisplayRectangle.Y + DisplayRectangle.Height/2),                                   // right tip (middle of box)
                new Point(DisplayRectangle.X + DisplayRectangle.Width - INDENT_SIZE, DisplayRectangle.Y + DisplayRectangle.Height - V_ADJ),               // bottom right of indented right "arrow"
                new Point(DisplayRectangle.X + INDENT_SIZE, DisplayRectangle.Y + DisplayRectangle.Height - V_ADJ),                                        // bottom left of indented left "arrow"
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

    public class AngleBracketBoxProperties : ElementProperties
    {
        [Category("Logic")]
        public TruePath TruePath { get; set; }

        public AngleBracketBoxProperties(AngleBracketBox el) : base(el)
        {
            TruePath = el.TruePath;
        }

        public override void Update(GraphicElement el, string label)
        {
            (label == nameof(TruePath)).If(() => ((AngleBracketBox)el).TruePath = TruePath);
            base.Update(el, label);
        }
    }
}
