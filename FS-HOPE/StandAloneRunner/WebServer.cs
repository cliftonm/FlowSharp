/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;
using Clifton.Core.Services.SemanticProcessorService;

namespace StandAloneRunner
{
    public class WebServer
    {
        protected HttpListener listener;
        protected SemanticProcessor sp;

        protected const string INSTANTIATE_RECEPTOR = "instantiateReceptor";
        protected const string INSTANTIATE_SEMANTIC_TYPE = "instantiateSemanticType";
        protected const string PUBLISH_SEMANTIC_TYPE = "publishSemanticType";
        protected const string ENABLE_DISABLE_RECEPTOR = "enableDisableReceptor";

        // TODO: Membrane needs to be added to the type name as well.
        protected Dictionary<string, IReceptor> receptors = new Dictionary<string, IReceptor>();

        public WebServer()
        {
            sp = new SemanticProcessor();
            sp.Processing += Processing;
        }

        public void Start(string ip, int[] ports)
        {
            listener = new HttpListener();

            foreach (int port in ports)
            {
                string url = IpWithPort(ip, port);
                listener.Prefixes.Add(url);
            }

            listener.Start();
            Task.Run(() => WaitForConnection(listener));
        }

        protected void WaitForConnection(object objListener)
        {
            HttpListener listener = (HttpListener)objListener;

            while (true)
            {
                // Wait for a connection.  Return to caller while we wait.
                HttpListenerContext context = listener.GetContext();

                // Redirect to HTTPS if not local and not secure.
                if (!context.Request.IsLocal && !context.Request.IsSecureConnection)
                {
                    string redirectUrl = context.Request.Url.ToString().Replace("http:", "https:");
                    context.Response.Redirect(redirectUrl);
                    context.Response.Close();
                }
                else
                {
                    string data = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();
                    ProcessRoute(context, data);
                }
            }
        }

        protected void ProcessRoute(HttpListenerContext context, string data)
        {
            Dictionary<string, Action<HttpListenerContext, string>> routes = new Dictionary<string, Action<HttpListenerContext, string>>()
            {
                {INSTANTIATE_RECEPTOR, InstantiateReceptor},
                {INSTANTIATE_SEMANTIC_TYPE, InstantiateSemanticType },
                {PUBLISH_SEMANTIC_TYPE, PublishSemanticType },
                {ENABLE_DISABLE_RECEPTOR, EnableDisableReceptor },
            };

            Program.form.Invoke(() =>
            {
                Program.tbLog.AppendText(context.Request.Url.ToString() + "\n");
                Program.tbLog.AppendText(data + "\n");
            });

            string route = context.Request.Url.ToString().RightOfRightmostOf('/').LeftOf('?');
            string parms = context.Request.Url.ToString().RightOf('?');
            Action<HttpListenerContext, string> handler;

            if (routes.TryGetValue(route, out handler))
            {
                try
                {
                    handler(context, parms);
                }
                catch (Exception ex)
                {
                    Program.form.BeginInvoke(() =>
                    {
                        Program.tbLog.AppendText(ex.Message + "\n");
                        Program.tbLog.AppendText(ex.StackTrace + "\n");
                    });
                }
            }

            context.Response.Close();
        }

        protected void InstantiateReceptor(HttpListenerContext context, string data)
        {
            string typeName = data.RightOf('=');
            InstantiateReceptor(typeName);

            Response(context, "OK", "text/text");
        }

        protected void InstantiateSemanticType(HttpListenerContext context, string data)
        {
            Type t = Type.GetType("App." + data.RightOf('=') + ", HelloWorld");
            ISemanticType st = (ISemanticType)Activator.CreateInstance(t);
            string json = JsonConvert.SerializeObject(st);
            Response(context, json, "text/json");
        }

        protected void PublishSemanticType(HttpListenerContext context, string data)
        {
            // TODO: Fix assumptions about ordering of params
            string[] parms = data.Split('&');
            Type t = Type.GetType("App." + parms[0].RightOf('=') + ", HelloWorld");
            ISemanticType st = (ISemanticType)JsonConvert.DeserializeObject(parms[1].RightOf('='), t);
            sp.ProcessInstance<HopeRunner.HopeMembrane>(st);
            Response(context, "OK", "text/text");
        }

        protected void EnableDisableReceptor(HttpListenerContext context, string data)
        {
            // TODO: Fix assumptions about ordering of params
            string[] parms = data.Split('&');
            string typeName = "App." + parms[0].RightOf('=');

            if (parms[1].RightOf('=').to_b())
            {
                // enable
                if (!receptors.ContainsKey(typeName))
                {
                    InstantiateReceptor(typeName);
                }
            }
            else
            {
                // disable
                IReceptor receptor;

                if (receptors.TryGetValue(typeName, out receptor))
                {
                    sp.Unregister<HopeRunner.HopeMembrane>(receptor);
                    receptors.Remove(typeName);
                }
            }

            Response(context, "OK", "text/text");
        }

        protected void Response(HttpListenerContext context, string resp, string contentType)
        {
            byte[] utf8data = Encoding.UTF8.GetBytes(resp);
            context.Response.ContentType = contentType;
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = utf8data.Length;
            context.Response.OutputStream.Write(utf8data, 0, utf8data.Length);
        }

        protected string IpWithPort(string ip, int port)
        {
            string ret;

            if (port == 80)
            {
                ret = "http://" + ip + "/";
            }
            else if ((ip == "localhost") || (ip == "127.0.0.1"))
            {
                ret = "http://" + ip + ":" + port.ToString() + "/";
            }
            else
            {
                ret = "https://" + ip + ":" + port.ToString() + "/";
            }

            return ret;
        }

        protected void Processing(object sender, ProcessEventArgs args)
        {
        }

        protected void InstantiateReceptor(string typeName)
        {
            Program.form.BeginInvoke(() =>
            {
                Type t = Type.GetType(typeName + ", HelloWorld");
                IReceptor receptor = (IReceptor)Activator.CreateInstance(t);
                sp.Register<HopeRunner.HopeMembrane>(receptor);
                receptors[typeName] = receptor;
            });
        }
    }
}