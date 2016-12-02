/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

// Inspired by a plugin that Lucas Martins da Silva created.

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

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

    public struct Cell      // Struct so it can be used as a lookup key in a dictionary.
    {
        public int Column;
        public int Row;

        public override string ToString()
        {
            return Column.ToString() + "," + Row.ToString();
        }
    }

    public class GridBox : GraphicElement
    {
        public int Columns { get; set; }
        public int Rows { get; set; }

        protected Dictionary<Cell, string> cellText;
        protected int editCol;
        protected int editRow;

        public GridBox(Canvas canvas) : base(canvas)
        {
            Columns = 4;
            Rows = 4;
            cellText = new Dictionary<Shapes.Cell, string>();
        }

        public override ElementProperties CreateProperties()
        {
            return new GridBoxProperties(this);
        }

        public override void Serialize(ElementPropertyBag epb, IEnumerable<GraphicElement> elementsBeingSerialized)
        {
            Json["columns"] = Columns.ToString();
            Json["rows"] = Rows.ToString();
            Json["textFields"] = cellText.Count.ToString();
            int n = 0;

            foreach (KeyValuePair<Cell, string> kvp in cellText)
            {
                Json["celltext" + n] = kvp.Key.ToString() + "," + kvp.Value;
                ++n;
            }

            base.Serialize(epb, elementsBeingSerialized);
        }

        public override void Deserialize(ElementPropertyBag epb)
        {
            base.Deserialize(epb);
            Columns = Json["columns"].to_i();
            Rows = Json["rows"].to_i();
            int cellTextCount = Json["textFields"].to_i();

            for (int i = 0; i < cellTextCount; i++)
            {
                string cellInfo = Json["celltext" + i];
                string[] cellData = cellInfo.Split(',');
                cellText[new Cell() { Column = cellData[0].to_i(), Row = cellData[1].to_i() }] = cellData[2];
            }
        }

        public override TextBox CreateTextBox(Point mousePosition)
        {
            TextBox tb;
            // Get cell where mouse cursor is currently over.
            Point localMousePos = Canvas.PointToClient(mousePosition);
            editCol = -1;
            editRow = -1;
            int cellWidth = DisplayRectangle.Width / Columns;
            int cellHeight = DisplayRectangle.Height / Rows;

            if (DisplayRectangle.Contains(localMousePos))
            {
                editCol = (localMousePos.X - DisplayRectangle.Left) / cellWidth;
                editRow = (localMousePos.Y - DisplayRectangle.Top) / cellHeight;
                tb = new TextBox();
                tb.Location = DisplayRectangle.TopLeftCorner().Move(editCol * cellWidth, editRow * cellHeight + cellHeight / 2 - 10);
                tb.Size = new Size(cellWidth, 20);
                string text;
                cellText.TryGetValue(new Cell() { Column = editCol, Row = editRow }, out text);
                tb.Text = text;
            }
            else
            {
                tb = base.CreateTextBox(mousePosition);
            }

            return tb;
        }

        public override void EndEdit(string newVal, string oldVal)
        {
            int editColClosure = editCol;
            int editRowClosure = editRow;
            string oldValClosure = "";
            Cell cell = new Cell() { Column = editColClosure, Row = editRowClosure };
            cellText.TryGetValue(cell, out oldValClosure);

            canvas.Controller.UndoStack.UndoRedo("Inline edit",
                () =>
                {
                    canvas.Controller.Redraw(this, (el) => cellText[cell] = newVal);
                    canvas.Controller.ElementSelected.Fire(this, new ElementEventArgs() { Element = this });
                },
                () =>
                {
                    canvas.Controller.Redraw(this, (el) => cellText[cell] = oldValClosure);
                    canvas.Controller.ElementSelected.Fire(this, new ElementEventArgs() { Element = this });
                });
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
            Brush brush = new SolidBrush(TextColor);

            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    string text;

                    if (cellText.TryGetValue(new Cell() { Column = x, Row = y }, out text))
                    {
                        SizeF size = gr.MeasureString(text, TextFont);
                        Point textpos;
                        Rectangle rectCell = new Rectangle(r.Left + cellWidth * x, r.Top + cellHeight * y, cellWidth, cellHeight);
                        textpos = rectCell.Center().Move((int)(-size.Width / 2), (int)(-size.Height / 2));
                        gr.DrawString(text, TextFont, brush, textpos);
                    }
                }
            }

            brush.Dispose();
            base.Draw(gr);
        }
    }
}
