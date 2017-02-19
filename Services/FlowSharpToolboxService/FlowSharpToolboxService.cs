/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
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
            // Initialize built-in shapes.
            // TODO: FlowSharpLib.dll can be a dll listed in the plugin list.
            string fslPath = Path.Combine(Application.ExecutablePath.LeftOfRightmostOf("\\"), "FlowSharpLib.dll");
            Assembly assy = Assembly.LoadFrom(fslPath);
            IEnumerable<Type> shapes = assy.GetTypes().Where(t => t.IsSubclassOf(typeof(GraphicElement)) && !t.IsAbstract);
            AddShapes(shapes);
        }

        public void InitializePluginsInToolbox()
        {
            PluginManager pluginManager = new PluginManager();
            pluginManager.InitializePlugins();
            IEnumerable<Type> pluginShapes = pluginManager.GetShapeTypes().Where(t => !t.IsAbstract);
            AddShapes(pluginShapes);
        }

        public void UpdateToolboxPaths()
        {
            toolboxController.Elements.ForEach(el => el.UpdatePath());
        }

        protected void AddShapes(IEnumerable<Type> shapes)
        {
            var orderedShapes = (from t in shapes
                                 select new
                                 {
                                     ShapeType = t,
                                     Order = t.GetCustomAttribute(typeof(ToolboxOrderAttribute)) == null ? 9999 : ((ToolboxOrderAttribute)t.GetCustomAttribute(typeof(ToolboxOrderAttribute))).Order,
                                 }).OrderBy(q => q.Order);


            orderedShapes.ForEach(t =>
            {
                // Show only shapes that are not excluded from the toolbox (which should use a GraphicElement with ToolboxShape metadata.
                if (t.ShapeType.GetCustomAttribute(typeof(ExcludeFromToolboxAttribute)) == null)
                {
                    GraphicElement el = Activator.CreateInstance(t.ShapeType, new object[] { toolboxCanvas }) as GraphicElement;
                    el.DisplayRectangle = el.ToolboxDisplayRectangle;
                    toolboxController.AddElement(el);
                }
            });
        }
    }

    public class FlowSharpToolboxReceptor : IReceptor
    {

    }
}
