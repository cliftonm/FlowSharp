/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.IO;
using System.Net;
using System.Threading.Tasks;

using Clifton.Core.ServiceManagement;

namespace FlowSharpRestService
{
    public class WebServer
    {
        protected HttpListener listener;
        protected Routes routes;
        protected IServiceManager serviceManager;

        public WebServer(IServiceManager serviceManager)
        {
            this.serviceManager = serviceManager;
        }

        public void Start(string ip, int[] ports)
        {
            routes = new Routes(serviceManager);
            routes.InitializeRoutes();
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
            routes.Route(context, data);
            context.Response.Close();
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
    }
}