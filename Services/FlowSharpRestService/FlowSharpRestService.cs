/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Linq;

using Clifton.Core.ExtensionMethods;
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

        public string HttpGet(string url, string data)
        {
            string ret = Http.Get(url + "?" + data);

            return ret;
        }

        public string HttpGet(string url, Dictionary<string, string> data)
        {
            string asParams = string.Join("&", data.Select(kvp => kvp.Key + "=" + kvp.Value));
            string ret = Http.Get(url + "?" + asParams);

            return ret;
        }

        protected void InitializeListener()
        {
            server = new WebServer(ServiceManager);
            server.Start("localhost", new int[] { 8001 });      // TODO: Get IP, ports, etc., from config file.
        }
    }
}