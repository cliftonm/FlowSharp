using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

using FlowSharpServiceInterfaces;

namespace FlowSharpRestService
{
    public class Route
    {
        public string Verb { get; set; }
        public string Path { get; set; }
    }

    public partial class Routes
    {
        protected Dictionary<Route, Action<HttpListenerContext, string>> routes;
        protected IServiceManager serviceManager;

        public Routes(IServiceManager serviceManager)
        {
            routes = new Dictionary<Route, Action<HttpListenerContext, string>>();
            this.serviceManager = serviceManager;
        }

        public void Route(HttpListenerContext context, string data)
        {
            Console.Write(context.Request.HttpMethod.ToUpper() + " ");
            Console.WriteLine(context.Request.RawUrl.LeftOf("?").RightOf("/").ToLower());
            KeyValuePair<Route, Action<HttpListenerContext, string>> route = routes.SingleOrDefault(kvp => kvp.Key.Verb == context.Request.HttpMethod.ToUpper() && kvp.Key.Path == context.Request.RawUrl.LeftOf("?").RightOf("/").ToLower());
            route.Value?.Invoke(context, data);
        }

        public void InitializeRoutes()
        {
            routes[new Route() { Verb = "GET", Path = "flowsharp" }] = PublishSemanticMessage;
        }

        protected void PublishSemanticMessage(HttpListenerContext context, string data)
        {
            NameValueCollection nvc = context.Request.QueryString;
            string stname = nvc["cmd"];
            Type st = Type.GetType("FlowSharpServiceInterfaces." + stname + ",FlowSharpServiceInterfaces");
            ISemanticType t = Activator.CreateInstance(st) as ISemanticType;
            PopulateType(t, nvc);
            serviceManager.Get<ISemanticProcessor>().ProcessInstance<FlowSharpMembrane>(t);
            Response(context);      // TODO: response might be something the proc returns, in which case it needs to be synchronous.
        }

        protected void PopulateType(ISemanticType packet, NameValueCollection nvc)
        {
            foreach (string key in nvc.AllKeys)
            {
                PropertyInfo pi = packet.GetType().GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (pi != null)
                {
                    object valOfType = Convert.ChangeType(Uri.UnescapeDataString(nvc[key].Replace('+', ' ')), pi.PropertyType);
                    pi.SetValue(packet, valOfType);
                }
            }
        }

        public void Response(HttpListenerContext context)
        {
            byte[] utf8data = Encoding.UTF8.GetBytes("OK");
            context.Response.ContentType = "text/html";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = utf8data.Length;
            context.Response.OutputStream.Write(utf8data, 0, utf8data.Length);
        }
    }
}
