/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ModuleManagement;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharpToolboxService
{
    public class FlowSharpCanvasModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpToolboxService, FlowSharpToolboxService>();
        }
    }

    public class FlowSharpToolboxService : ServiceBase, IFlowSharpToolboxService
    {
        public BaseController Controller { get { return toolboxController; } }
        protected ToolboxCanvas toolboxCanvas;
        protected ToolboxController toolboxController;
        protected Control pnlToolbox;

        public override void Initialize(IServiceManager svcMgr)
        {
            base.Initialize(svcMgr);
            ServiceManager.Get<ISemanticProcessor>().Register<FlowSharpMembrane, FlowSharpToolboxReceptor>();
        }

        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
        }

        public void CreateToolbox(Control parent)
        {
            pnlToolbox = parent;
            toolboxCanvas = new ToolboxCanvas();
            toolboxController = new ToolboxController(ServiceManager, toolboxCanvas);
            toolboxCanvas.Controller = toolboxController;
            toolboxCanvas.Initialize(parent);
        }

        public void ResetDisplacement()
        {
            toolboxController.ResetDisplacement();
        }

        public void InitializeToolbox()
        {
            int x = pnlToolbox.Width / 2 - 12;
            toolboxController.AddElement(new Box(toolboxCanvas) { DisplayRectangle = new Rectangle(x - 50, 15, 25, 25) });
            toolboxController.AddElement(new Ellipse(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 15, 25, 25) });
            toolboxController.AddElement(new Diamond(toolboxCanvas) { DisplayRectangle = new Rectangle(x + 50, 15, 25, 25) });

            toolboxController.AddElement(new LeftTriangle(toolboxCanvas) { DisplayRectangle = new Rectangle(x - 60, 60, 25, 25) });
            toolboxController.AddElement(new RightTriangle(toolboxCanvas) { DisplayRectangle = new Rectangle(x - 20, 60, 25, 25) });
            toolboxController.AddElement(new UpTriangle(toolboxCanvas) { DisplayRectangle = new Rectangle(x + 20, 60, 25, 25) });
            toolboxController.AddElement(new DownTriangle(toolboxCanvas) { DisplayRectangle = new Rectangle(x + 60, 60, 25, 25) });

            toolboxController.AddElement(new HorizontalLine(toolboxCanvas) { DisplayRectangle = new Rectangle(x - 50, 130, 30, 20) });
            toolboxController.AddElement(new VerticalLine(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 125, 20, 30) });
            toolboxController.AddElement(new DiagonalConnector(toolboxCanvas, new Point(x + 50, 125), new Point(x + 50 + 25, 125 + 25)));

            toolboxController.AddElement(new DynamicConnectorLR(toolboxCanvas, new Point(x - 50, 175), new Point(x - 50 + 25, 175 + 25)));
            toolboxController.AddElement(new DynamicConnectorLD(toolboxCanvas, new Point(x, 175), new Point(x + 25, 175 + 25)));
            toolboxController.AddElement(new DynamicConnectorUD(toolboxCanvas, new Point(x + 50, 175), new Point(x + 50 + 25, 175 + 25)));

            toolboxController.AddElement(new ToolboxText(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 230, 25, 25) });
        }

        public void InitializePluginsInToolbox()
        {
            PluginManager pluginManager = new PluginManager();
            pluginManager.InitializePlugins();

            int x = pnlToolbox.Width / 2 - 12;
            List<Type> pluginShapes = pluginManager.GetShapeTypes();

            // Plugin shapes
            int n = x - 60;
            int y = 260;

            foreach (Type t in pluginShapes)
            {
                GraphicElement pluginShape = Activator.CreateInstance(t, new object[] { toolboxCanvas }) as GraphicElement;
                pluginShape.DisplayRectangle = new Rectangle(n, y, 25, 25);
                toolboxController.AddElement(pluginShape);

                // Next toolbox shape position:
                n += 40;

                if (n > x + 60)
                {
                    n = x - 60;
                    y += 40;
                }
            }
        }

        public void UpdateToolboxPaths()
        {
            toolboxController.Elements.ForEach(el => el.UpdatePath());
        }
    }

    public class FlowSharpToolboxReceptor : IReceptor
    {

    }
}
