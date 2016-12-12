/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Drawing;

using FlowSharpLib;

namespace FlowSharpCodeShapes
{
    public class Receptor : Ellipse
    {
        protected string receptorTemplate =
@"
using Clifton.Core.ServiceInterfaces;
using Clifton.Core.ServiceManagement;

// using [YourSemanticTypeNamespace];

namespace YourReceptorNamespace
{
	public class YourReceptor : IReceptor
	{
        protected IServiceManager serviceManager;

        public YourReceptor(IServiceManager serviceManager)
        {
            this.serviceManager = serviceManager;
            serviceManager.Get<ISemanticProcessor>().Register<Membrane, YourReceptor>();
        }

        // Change ISemanticType to the specific semantic message to process...
		public void Process(ISemanticProcessor proc, IMembrane membrane, ISemanticType msg)
		{
			// Process the message...
		}
    }
}
";

        public Receptor(Canvas canvas) : base(canvas)
        {
            Text = "Rcptr";
            TextFont.Dispose();
            TextFont = new Font(FontFamily.GenericSansSerif, 6);
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
            GraphicElement el = base.CloneDefault(canvas, offset);
            el.Text = "Receptor";
            el.Json["Code"] = receptorTemplate;

            return el;
        }
    }
}