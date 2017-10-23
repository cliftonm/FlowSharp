using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;

using Newtonsoft.Json;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ServiceManagement;

using FlowSharpCodeServiceInterfaces;
using FlowSharpHopeCommon;
using FlowSharpServiceInterfaces;

namespace FlowSharpHopeService
{
    public class StandAloneRunner : IRunner
    {
        public event EventHandler<HopeRunnerAppDomainInterface.ProcessEventArgs> Processing;
        public bool Loaded { get { return loaded; } }

        protected IServiceManager serviceManager;
        protected Process process;
        protected string url = "http://localhost:5001/";
        protected const string INSTANTIATE_RECEPTOR = "instantiateReceptor";
        protected const string INSTANTIATE_SEMANTIC_TYPE = "instantiateSemanticType";
        protected const string DESCRIBE_SEMANTIC_TYPE = "describeSemanticType";
        protected const string DESCRIBE_RECEPTOR = "describeReceptor";
        protected const string PUBLISH_SEMANTIC_TYPE = "publishSemanticType";
        protected const string ENABLE_DISABLE_RECEPTOR = "enableDisableReceptor";
        protected const string CLOSE = "close";
        protected bool loaded = false;
		protected WebServer webServer;

		public StandAloneRunner(IServiceManager serviceManager)
        {
			this.serviceManager = serviceManager;
			webServer = new WebServer(new RouteHandlers(this));
			webServer.Start("localhost", new int[] { 5002 });
		}

		public void Load(string fullName)
        {
			IFlowSharpCodeService codeSvc = serviceManager.Get<IFlowSharpCodeService>();
			process = codeSvc.LaunchProcess(fullName, String.Empty, _ => { });
			loaded = true;
		}

        public void Unload()
		{
            //IFlowSharpCodeService codeSvc = serviceManager.Get<IFlowSharpCodeService>();
            //codeSvc.TerminateProcess(process);
            if (loaded)
            {
                IFlowSharpRestService restSvc = serviceManager.Get<IFlowSharpRestService>();
                restSvc.HttpGet(url + CLOSE, "");
                process.WaitForExit();
            }

            process = null;
			loaded = false;
		}

        public void InstantiateReceptor(string name)
        {
            IFlowSharpRestService restSvc = serviceManager.Get<IFlowSharpRestService>();
            // TODO: Fix the hardcoded "App." -- figure out some way of getting the namespace?
            restSvc.HttpGet(url + INSTANTIATE_RECEPTOR, "receptorTypeName=" + "App." + name);
        }

        public List<ReceptorDescription> DescribeReceptor(string typeName)
        {
            IFlowSharpRestService restSvc = serviceManager.Get<IFlowSharpRestService>();
            string json = restSvc.HttpGet(url + DESCRIBE_RECEPTOR, "receptorName=" + typeName);
            var ret = JsonConvert.DeserializeObject<List<ReceptorDescription>>(json);

            return ret;
        }

        public PropertyContainer DescribeSemanticType(string typeName)
        {
            IFlowSharpRestService restSvc = serviceManager.Get<IFlowSharpRestService>();
            string json = restSvc.HttpGet(url + DESCRIBE_SEMANTIC_TYPE, "semanticTypeName=" + typeName);
            var ret = JsonConvert.DeserializeObject<PropertyContainer>(json);

            return ret;

            // Amazing, but needs a bunch of type descriptor support to display and Expando object on a property grid.
            // See HopeShapesPublishSemanticType.cs for implementation.
            //         var converter = new Newtonsoft.Json.Converters.ExpandoObjectConverter();
            //dynamic inst = JsonConvert.DeserializeObject<ExpandoObject>(ret, converter);

            // return inst;
        }

        public void Publish(string typeName, object jsonObject)
        {
            string json = JsonConvert.SerializeObject(jsonObject);

            Publish(typeName, json);
        }

        public void Publish(string typeName, string json)
        {
            IFlowSharpRestService restSvc = serviceManager.Get<IFlowSharpRestService>();
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                {"semanticTypeName", typeName },
                {"instanceJson", json },
            };

            restSvc.HttpGet(url + PUBLISH_SEMANTIC_TYPE, data);
        }

        public void EnableDisableReceptor(string typeName, bool state)
        {
            // Loading an FSD or adding agents while the stand alone runner is running will immediately
            // instantiate the receptor.  In the former case, we don't really want this, as the the stand-alone runner
            // may not be instantiated yet.  In the latter case, the user has the choice to stop the stand-alone runner
            // before creating more agent receptors.
            if (loaded)
            {
                IFlowSharpRestService restSvc = serviceManager.Get<IFlowSharpRestService>();
                // TODO: Membrane is also required so we manipulate the correct receptor.
                restSvc.HttpGet(url + ENABLE_DISABLE_RECEPTOR, "receptorTypeName=" + typeName + "&state=" + state);
            }
        }

		public void ProcessMessage(HopeRunnerAppDomainInterface.ProcessEventArgs stMsg)
		{
			Processing.Fire(this, stMsg);
		}
	}
}
