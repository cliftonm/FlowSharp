/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using FlowSharpLib;

namespace FlowSharpWindowsControlShapes
{
    public abstract class ControlShape : Box
    {
        public string ClickEventName { get; set; }
        public string ClickEventData { get; set; }
        public bool Enabled { get; set; }
        public bool Visible { get; set; }

        protected Control control;

        public ControlShape(Canvas canvas) : base(canvas)
        {
            ClickEventName = "ButtonClick";     // Default.
            Enabled = true;
            Visible = true;
        }

        public override ElementProperties CreateProperties()
        {
            return new ControlShapeProperties(this);
        }

        public override void DrawText(Graphics gr)
        {
            // Do nothing, as we don't display the text in the shape itself, only the edit control.
        }

        public override void Move(Point delta)
        {
            // Prevents trails being left by controls when canvas is dragged.
            base.Move(delta);
            control.Visible = OnScreen();
        }
        public override void Serialize(ElementPropertyBag epb, IEnumerable<GraphicElement> elementsBeingSerialized)
        {
            Json["ClickEventName"] = ClickEventName;
            Json["ClickEventData"] = ClickEventData;
            base.Serialize(epb, elementsBeingSerialized);
        }

        public override void Deserialize(ElementPropertyBag epb)
        {
            base.Deserialize(epb);
            string name;

            if (Json.TryGetValue("ClickEventName", out name))
            {
                ClickEventName = name;
            }

            string data;

            if (Json.TryGetValue("ClickEventData", out data))
            {
                ClickEventData = data;
            }
        }

        public override void Removed(bool dispose)
        {
            base.Removed(dispose);
            control.Visible = false;

            // Detach the control if we are actually disposing.
            if (dispose)
            {
                control.Parent = null;
            }
        }

        public override void Restored()
        {
            base.Restored();
            control.Visible = Visible;
        }

        protected string AppendData(string url)
        {
            if (!string.IsNullOrEmpty(ClickEventData))
            {
                url += "&" + ClickEventData;
            }

            return url;
        }
    }
}
