/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Windows.Forms;

using FlowSharpLib;

namespace FlowSharpWindowsControlShapes
{
    public abstract class ControlShape : Box
    {
        public string ClickEventName { get; set; }
        public string ClickEventData { get; set; }

        protected Control control;

        public ControlShape(Canvas canvas) : base(canvas)
        {
            ClickEventName = "ButtonClick";     // Default.
        }

        public override ElementProperties CreateProperties()
        {
            return new ControlShapeProperties(this);
        }

        public override void Erase()
        {
            base.Erase();
            control.Hide();
        }

        public override void Draw()
        {
            base.Draw();
            control.Show();
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
