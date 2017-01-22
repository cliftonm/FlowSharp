/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using Newtonsoft.Json.Linq;

using FlowSharpLib;

// localhost:8001/flowsharp?cmd=CmdUpdateProperty&Name=cbItems&PropertyName=JsonItems&Value=[{'id':'0','name':'foo'},{'id':'1','name':'fiz bin'}]

namespace FlowSharpWindowsControlShapes
{
    public class ListBoxItem
    {
        public object Id { get; set; }
        public object Display { get; set; }
    }

    [ExcludeFromToolbox]
    public class ListBoxShape : ControlShape
    {
        protected string jsonItems;

        public string IdFieldName { get; set; }
        public string DisplayFieldName { get; set; }

        public string JsonItems
        {
            get { return jsonItems; }
            set
            {
                jsonItems = value;
                UpdateList();
            }
        }

        public ListBoxShape(Canvas canvas) : base(canvas)
        {
            ListBox lb = new ListBox();
            control = lb;
            canvas.Controls.Add(control);
            lb.ValueMember = "Id";
            lb.DisplayMember = "Display";
        }

        public override ElementProperties CreateProperties()
        {
            return new ListBoxShapeProperties(this);
        }

        public override void Serialize(ElementPropertyBag epb, IEnumerable<GraphicElement> elementsBeingSerialized)
        {
            Json["IdFieldName"] = IdFieldName;
            Json["DisplayFieldName"] = DisplayFieldName;
            base.Serialize(epb, elementsBeingSerialized);
        }

        public override void Deserialize(ElementPropertyBag epb)
        {
            base.Deserialize(epb);
            string idFieldName;
            string valueFieldName;
            Json.TryGetValue("IdFieldName", out idFieldName);
            Json.TryGetValue("DisplayFieldName", out valueFieldName);

            DisplayFieldName = valueFieldName;
            IdFieldName = idFieldName;
        }

        public override void Draw(Graphics gr)
        {
            base.Draw(gr);
            Rectangle r = DisplayRectangle.Grow(-4);

            if (control.Location != r.Location)
            {
                control.Location = r.Location;
            }

            if (control.Size != r.Size)
            {
                control.Size = r.Size;
            }

            if (control.Enabled != Enabled)
            {
                control.Enabled = Enabled;
            }

            if (control.Visible != Visible)
            {
                control.Visible = Visible;
            }
        }

        /// <summary>
        /// Map "Id" and "Display" in ListBoxItem to the ID and display field names in the JSON.
        /// </summary>
        private void UpdateList()
        {
            ListBox lb = (ListBox)control;
            lb.Items.Clear();

            dynamic items = JArray.Parse(JsonItems);
            List<ListBoxItem> cbItems = new List<ListBoxItem>();

            foreach (var item in items)
            {
                ListBoxItem cbItem = new ListBoxItem();
                cbItem.Id = item[IdFieldName];
                cbItem.Display = item[DisplayFieldName].ToString();
                cbItems.Add(cbItem);
            }

            lb.Items.AddRange(cbItems.ToArray());
            lb.SelectedIndex = 0;
        }
    }

    [ToolboxShape]
    public class ToolboxListBoxShape : GraphicElement
    {
        public const string TOOLBOX_TEXT = "lbbox";

        protected Brush brush = new SolidBrush(Color.Black);

        public ToolboxListBoxShape(Canvas canvas) : base(canvas)
        {
            TextFont.Dispose();
            TextFont = new Font(FontFamily.GenericSansSerif, 8);
        }

        public override GraphicElement CloneDefault(Canvas canvas)
        {
            return CloneDefault(canvas, Point.Empty);
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
            ListBoxShape shape = new ListBoxShape(canvas);
            shape.DisplayRectangle = shape.DefaultRectangle().Move(offset);
            shape.UpdateProperties();
            shape.UpdatePath();

            return shape;
        }

        public override void Draw(Graphics gr)
        {
            SizeF size = gr.MeasureString(TOOLBOX_TEXT, TextFont);
            Point textpos = DisplayRectangle.Center().Move((int)(-size.Width / 2), (int)(-size.Height / 2));
            gr.DrawString(TOOLBOX_TEXT, TextFont, brush, textpos);
            base.Draw(gr);
        }
    }
}
