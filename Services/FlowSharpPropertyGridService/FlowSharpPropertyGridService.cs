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

namespace FlowSharpPropertyGridService
{
    public class FlowSharpPropertyGridSModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpPropertyGridService, FlowSharpPropertyGridService>();
        }
    }

    public class FlowSharpPropertyGridService : ServiceBase, IFlowSharpPropertyGridService
    {
        protected PropertyGridController pgController;

        public override void Initialize(IServiceManager svcMgr)
        {
            base.Initialize(svcMgr);
        }

        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
        }

        public void Initialize(PropertyGrid propertyGrid)
        {
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
            pgController = new PropertyGridController(propertyGrid, canvasController);
        }
    }

    public class FlowSharpPropertyGridReceptor : IReceptor
    {

    }
}
