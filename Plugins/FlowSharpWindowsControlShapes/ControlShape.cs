/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharpWindowsControlShapes
{
    public abstract class ControlShape : Box
    {
        public string ClickEventName { get; set; }
        public string ClickEventData { get; set; }
        public bool Enabled { get; set; }
        public bool Visible { get; set; }
        public SendProtocol SendProtocol { get; set; }

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
            Json["SendProtocol"] = SendProtocol.ToString();
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

            string sendProtocol;

            if (Json.TryGetValue("SendProtocol", out sendProtocol))
            {
                SendProtocol = (SendProtocol)Enum.Parse(typeof(SendProtocol), sendProtocol);
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

        protected void Send(string cmd)
        {
            // This allows the user to configure, for each control, whether it sends a web socket or HTTP message.
            // We also assume for now that it is best to send these messages synchronously, so that order is preserved.
            switch (SendProtocol)
            {
                case SendProtocol.Http:
                    {
                        string url = "http://localhost:8002/" + cmd;
                        string data = "ShapeName=" + Name;
                        data = AppendData(data);
                        ServiceManager.Instance.Get<ISemanticProcessor>().ProcessInstance<FlowSharpMembrane, HttpSend>(d =>
                        {
                            d.Url = url;
                            d.Data = data;
                        }, true);
                        break;
                    }
                case SendProtocol.WebSocket:
                    {
                        string data = "cmd=" + cmd + "&ShapeName=" + Name;
                        data = AppendData(data);
                        ServiceManager.Instance.Get<ISemanticProcessor>().ProcessInstance<FlowSharpMembrane, WebSocketSend>(d =>
                        {
                            d.Data = data;
                        }, true);
                        break;
                    }
            }
        }

        protected virtual string AppendData(string data)
        {
            if (!string.IsNullOrEmpty(ClickEventData))
            {
                data += "&" + ClickEventData;
            }

            return data;
        }
    }
}
