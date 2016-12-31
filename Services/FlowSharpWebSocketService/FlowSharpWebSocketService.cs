/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Linq;
using System.Net;

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

        public void StartServer()
        {
            string address = "127.0.0.1";
            int port = 1100;
            IPAddress ipaddr = new IPAddress(address.Split('.').Select(a => Convert.ToByte(a)).ToArray());
            wss = new WebSocketServer(ipaddr, port, null);
            wss.AddWebSocketService<Server>("/game");
            wss.Start();
        }

        public void StopServer()
        {
        }

        public void Send(string msg)
        {
        }
    }

    public class Server : WebSocketBehavior
    {
    }
}