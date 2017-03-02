/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

using WebSocketSharp;
using WebSocketSharp.Server;

using Clifton.Core.ModuleManagement;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

using FlowSharpServiceInterfaces;

namespace FlowSharpWebSocketService
{
    public class FlowSharpWebSocketModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpWebSocketService, FlowSharpWebSocketService>();
        }
    }

    public class FlowSharpWebSocketService : ServiceBase, IFlowSharpWebSocketService
    {
        protected WebSocketServer wss;

        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
            ServiceManager.Get<ISemanticProcessor>().Register<FlowSharpMembrane, WebSocketSender>();
            StartServer();
        }

        public void StartServer()
        {
            List<IPAddress> ips = GetLocalHostIPs();
            string address = ips[0].ToString();
            int port = 1100;
            IPAddress ipaddr = new IPAddress(address.Split('.').Select(a => Convert.ToByte(a)).ToArray());
            wss = new WebSocketServer(ipaddr, port, null);
            wss.AddWebSocketService<Server>("/flowsharp");
            wss.Start();
        }

        public void StopServer()
        {
            wss.Stop();
        }

        protected List<IPAddress> GetLocalHostIPs()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            List<IPAddress> ret = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();

            return ret;
        }
    }

    public class Server : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.Type == Opcode.Text)
            {
                string msg = e.Data;
                Dictionary<string, string> data = ParseMessage(msg);
                string jsonResp = PublishSemanticMessage(data);

                if (jsonResp != null)
                {
                    Send(jsonResp);
                }
            }
        }

        protected Dictionary<string, string> ParseMessage(string msg)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();

            string[] dataPackets = msg.Split('&');

            foreach (string dp in dataPackets)
            {
                string[] varValue = dp.Split('=');
                data[varValue[0]] = varValue[1];
            }

            return data;
        }

        protected string PublishSemanticMessage(Dictionary<string, string> data)
        {
            string ret = null;
            Type st = Type.GetType("FlowSharpServiceInterfaces." + data["cmd"] + ",FlowSharpServiceInterfaces");
            ISemanticType t = Activator.CreateInstance(st) as ISemanticType;
            PopulateType(t, data);
            // Synchronous, because however we're processing the commands in order, otherwise we lose the point of a web socket,
            // which keeps the messages in order.
            ServiceManager.Instance.Get<ISemanticProcessor>().ProcessInstance<FlowSharpMembrane>(t, true);

            if (t is IHasResponse)
            {
                ret = ((IHasResponse)t).SerializeResponse();
            }

            return ret;
        }

        protected void PopulateType(ISemanticType packet, Dictionary<string, string> data)
        {
            foreach (string key in data.Keys)
            {
                PropertyInfo pi = packet.GetType().GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (pi != null)
                {
                    object valOfType = Convert.ChangeType(data[key], pi.PropertyType);
                    pi.SetValue(packet, valOfType);
                }
            }
        }
    }
}
