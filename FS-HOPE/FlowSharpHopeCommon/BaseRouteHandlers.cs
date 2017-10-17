using System;
using System.Collections.Generic;
using System.Net;

namespace FlowSharpHopeCommon
{
	public abstract class BaseRouteHandlers
	{
		public Dictionary<string, Func<HttpListenerContext, string, (string text, string mime)>> Routes { get { return routes; } }
		protected Dictionary<string, Func<HttpListenerContext, string, (string text, string mime)>> routes;
	}
}
