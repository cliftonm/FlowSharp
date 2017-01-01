/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using WebSocketSharp;

using Clifton.Core.Semantics;

using FlowSharpServiceInterfaces;

namespace FlowSharpWebSocketService
{
    public class MyListener { }

    public class WebSocketSender : IReceptor
    {
        public static WebSocket ws = null;

        public void Process(ISemanticProcessor proc, IMembrane membrane, WebSocketSend cmd)
        {
            EstablishConnection();
            ws.Send(cmd.Data);
        }

        protected void EstablishConnection()
        {
            // TODO: Right now, we're assuming one web socket client.
            if (ws == null)
            {
                ws = new WebSocket("ws://127.0.0.1:1101/flowsharpapp", new MyListener());
                ws.Connect();
            }

            // TODO: How do we close the connection???
            // TODO: How do we recover from a broken connection?
        }
    }
}