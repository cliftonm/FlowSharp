/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Clifton.Core.ExtensionMethods;

namespace FlowSharpHopeCommon
{
    public class WebServer
    {
        protected HttpListener listener;
		protected BaseRouteHandlers routeHandlers;

        public WebServer(BaseRouteHandlers routeHandlers)
        {
			this.routeHandlers = routeHandlers;
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
            //Program.form.Invoke(() =>
            //{
            //    Program.tbLog.AppendText(context.Request.Url.ToString() + "\n");
            //    Program.tbLog.AppendText(data + "\n");
            //});

            string route = context.Request.Url.ToString().RightOfRightmostOf('/').LeftOf('?');
            string parms = context.Request.Url.ToString().RightOf('?');
            Func<HttpListenerContext, string, (string text, string mime)> handler;

            if (routeHandlers.Routes.TryGetValue(route, out handler))
            {
                try
                {
                    var (text, mime) = handler(context, parms);
					Response(context, text, mime);
                }
                catch (Exception ex)
                {
                    //Program.form.BeginInvoke(() =>
                    //{
                    //    Program.tbLog.AppendText(ex.Message + "\n");
                    //    Program.tbLog.AppendText(ex.StackTrace + "\n");
                    //});
                }
            }

            context.Response.Close();
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
    }
}