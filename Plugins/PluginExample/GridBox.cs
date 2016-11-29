/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

// Inspired by a plugin that Lucas Martins da Silva created.

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

using Clifton.Core.ExtensionMethods;

namespace FlowSharpLib.Shapes
{
    public class GridBoxProperties : ElementProperties
    {
        [Category("Dimensions")]
        public int Columns { get; set; }
        [Category("Dimensions")]
        public int Rows { get; set; }

        public GridBoxProperties(GridBox el) : base(el)
        {
            Columns = el.Columns;
            Rows = el.Rows;
        }

        public override void Update(GraphicElement el, string label)
        {
            (label == nameof(Columns)).If(() => ((GridBox)el).Columns = Columns);
            (label == nameof(Rows)).If(() => ((GridBox)el).Rows = Rows);
            base.Update(el, label);
        }
    }

    public class GridBox : GraphicElement
    {
        public int Columns { get; set; }
        public int Rows { get; set; }

        public GridBox(Canvas canvas) : base(canvas)
        {
            Columns = 4;
            Rows = 4;
        }

        public override ElementProperties CreateProperties()
        {
            return new GridBoxProperties(this);
        }

        public override void Serialize(ElementPropertyBag epb, IEnumerable<GraphicElement> elementsBeingSerialized)
        {
            Json["columns"] = Columns.ToString();
            Json["rows"] = Rows.ToString();
            base.Serialize(epb, elementsBeingSerialized);
        }

        public override void Deserialize(ElementPropertyBag epb)
        {
            base.Deserialize(epb);
            Columns = Json["columns"].to_i();
            Rows = Json["rows"].to_i();
        }

        public override void Draw(Graphics gr)
        {
            Rectangle r = DisplayRectangle;
            int cellWidth = DisplayRectangle.Width / Columns;
            int cellHeight = DisplayRectangle.Height / Rows;
            RectangleF[] rects = new RectangleF[Rows * Columns];
            int n = 0;

            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    rects[n++] = new RectangleF(r.Left + cellWidth * x, r.Top + cellHeight * y, cellWidth, cellHeight);
                }
            }

            gr.FillRectangle(FillBrush, DisplayRectangle);
            gr.DrawRectangles(BorderPen, rects);

            base.Draw(gr);
        }
    }
}
