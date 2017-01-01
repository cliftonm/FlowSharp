/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using Clifton.Core.ModuleManagement;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

using FlowSharpServiceInterfaces;

namespace FlowSharpRestService
{
    public class FlowSharpRestModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpRestService, FlowSharpRestService>();
        }
    }

    public class FlowSharpRestService : ServiceBase, IFlowSharpRestService
    {
        protected WebServer server;

        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
            InitializeListener();
            ServiceManager.Get<ISemanticProcessor>().Register<FlowSharpMembrane, CommandProcessor>();
            ServiceManager.Get<ISemanticProcessor>().Register<FlowSharpMembrane, HttpSender>();
        }

        protected void InitializeListener()
        {
            server = new WebServer(ServiceManager);
            server.Start("localhost", new int[] { 8001 });      // TODO: Get IP, ports, etc., from config file.
        }
    }
}