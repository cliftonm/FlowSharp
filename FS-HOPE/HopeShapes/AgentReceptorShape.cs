/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpHopeShapeInterfaces;
using FlowSharpHopeServiceInterfaces;

namespace FlowSharpCodeShapes
{
    public class AgentReceptorProperties : ShapeProperties
    {
        [Category("Agent")]
        public string AgentName { get; set; }
        [Category("Agent")]
        public bool Enabled { get; set; }

        public AgentReceptorProperties(AgentReceptorShape el) : base(el)
        {
            AgentName = el.AgentName;
            Enabled = el.Enabled;
        }

        public override void Update(GraphicElement el, string label)
        {
            (label == nameof(AgentName)).If(() => ((AgentReceptorShape)el).AgentName = AgentName);
            (label == nameof(Enabled)).If(() => ((AgentReceptorShape)el).Enabled = Enabled);
            base.Update(el, label);
        }
    }

    public class AgentReceptorShape : Ellipse, IAgentReceptor
    {
        protected bool enabled;

        public string AgentName { get; set; }
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                FillBrush.Color = enabled ? Color.LightGreen : Color.Red;
                Redraw();
                IServiceManager serviceManager = canvas.ServiceManager;
                IHigherOrderProgrammingService hope = serviceManager.Get<IHigherOrderProgrammingService>();
                hope.EnableDisableReceptor(Text, enabled);
            }
        }

        public AgentReceptorShape(Canvas canvas) : base(canvas)
        {
            enabled = true;
            Text = "Rcptr";
            TextFont.Dispose();
            TextFont = new Font(FontFamily.GenericSansSerif, 6);
            FillBrush.Color = Color.LightGreen;
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
            enabled = true;
            GraphicElement el = base.CloneDefault(canvas, offset);
            el.Text = "Rcptr";
            FillBrush.Color = Color.LightGreen;

            return el;
        }

        public override ElementProperties CreateProperties()
        {
            return new AgentReceptorProperties(this);
        }

        public override void Serialize(ElementPropertyBag epb, IEnumerable<GraphicElement> elementsBeingSerialized)
        {
            Json["agentName"] = AgentName;
            Json["agentEnabled"] = Enabled.ToString();
            base.Serialize(epb, elementsBeingSerialized);
        }

        public override void Deserialize(ElementPropertyBag epb)
        {
            base.Deserialize(epb);
            AgentName = Json["agentName"];
            string strEnabled;

            if (Json.TryGetValue("agentEnabled", out strEnabled))
            {
                Enabled = Json["agentEnabled"].to_b();
            }
        }
    }
}
