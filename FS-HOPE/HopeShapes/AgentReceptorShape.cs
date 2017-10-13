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

    [ExcludeFromToolbox]
    public class AgentReceptorShape : GraphicElement, IAgentReceptor
    {
        protected Point[] path;
        protected bool enabled;

        public string AgentName { get; set; }
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                FillBrush.Color = enabled ? Color.LightGreen : Color.FromArgb(255, 80, 80);
                Redraw();
                IServiceManager serviceManager = canvas.ServiceManager;
                IHigherOrderProgrammingService hope = serviceManager.Get<IHigherOrderProgrammingService>();
                hope.EnableDisableReceptor(Text.Replace("\r", "").Replace("\n", ""), enabled);
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

    [ToolboxShape]
    [ToolboxOrder(10)]
    public class ToolboxAssemblyAgentReceptorShape : AgentReceptorShape
    {
        public override Rectangle ToolboxDisplayRectangle { get { return new Rectangle(0, 0, 35, 25); } }

        public ToolboxAssemblyAgentReceptorShape(Canvas canvas) : base(canvas)
        {
            Text = "Rcptr";
            TextFont.Dispose();
            TextFont = new Font(FontFamily.GenericSansSerif, 6);
            FillBrush.Color = Color.LightGreen;
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
