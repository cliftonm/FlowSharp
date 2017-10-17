using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Clifton.Core.ExtensionMethods;

using FlowSharpHopeCommon;

namespace FlowSharpHopeService
{
	public class RouteHandlers : BaseRouteHandlers
	{
		protected const string PROCESSING = "processing";
		protected StandAloneRunner runner;

		public RouteHandlers(StandAloneRunner runner)
		{
			this.runner = runner;

			routes = new Dictionary<string, Func<HttpListenerContext, string, (string text, string mime)>>()
			{
				{PROCESSING, Processing},
			};
		}

		protected (string text, string mime) Processing(HttpListenerContext context, string data)
		{
			var stMsg = JsonConvert.DeserializeObject<HopeRunnerAppDomainInterface.ProcessEventArgs>(data.RightOf('='));
			runner.ProcessMessage(stMsg);

			return ("OK", "text/text");
		}
	}
}
