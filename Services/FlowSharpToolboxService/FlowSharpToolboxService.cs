/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Windows.Forms;

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

        public override void Initialize(IServiceManager svcMgr)
        {
            base.Initialize(svcMgr);
            ServiceManager.Get<ISemanticProcessor>().Register<FlowSharpMembrane, FlowSharpToolboxReceptor>();
            toolboxCanvas = new ToolboxCanvas();
        }

        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().Controller;
            toolboxController = new ToolboxController(toolboxCanvas, canvasController);
            toolboxCanvas.Controller = toolboxController;
        }

        public void CreateToolbox(Control parent)
        {
            toolboxCanvas.Initialize(parent);
        }

        public void ResetDisplacement()
        {
            toolboxController.ResetDisplacement();
        }
    }

    public class FlowSharpToolboxReceptor : IReceptor
    {

    }
}
