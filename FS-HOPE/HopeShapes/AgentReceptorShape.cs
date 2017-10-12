/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

using Clifton.Core.ExtensionMethods;

using FlowSharpLib;
using FlowSharpHopeShapeInterfaces;

namespace FlowSharpCodeShapes
{
    public class AgentReceptorProperties : ShapeProperties
    {
        [Category("Agent")]
        public string AgentName { get; set; }

        public AgentReceptorProperties(AgentReceptorShape el) : base(el)
        {
            AgentName = el.AgentName;
        }

        public override void Update(GraphicElement el, string label)
        {
            (label == nameof(AgentName)).If(() => ((AgentReceptorShape)el).AgentName = AgentName);
            base.Update(el, label);
        }
    }

    public class AgentReceptorShape : Ellipse, IAgentReceptor
    {
        public string AgentName { get; set; }

        public AgentReceptorShape(Canvas canvas) : base(canvas)
        {
            Text = "Rcptr";
            TextFont.Dispose();
            TextFont = new Font(FontFamily.GenericSansSerif, 6);
            FillBrush.Color = Color.LightGreen;
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
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
            base.Serialize(epb, elementsBeingSerialized);
        }

        public override void Deserialize(ElementPropertyBag epb)
        {
            base.Deserialize(epb);
            AgentName = Json["agentName"];
        }
    }
}
