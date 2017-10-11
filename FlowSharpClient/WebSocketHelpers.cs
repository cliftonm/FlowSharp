/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using WebSocketSharp;

using FlowSharpLib;

namespace FlowSharpClient
{
	public class MyListener { }

	public static partial class WebSocketHelpers
	{
		private static WebSocket ws;
		private static string response;

		// cmd=CmdUpdateProperty&Name=btnTest&PropertyName=Text&Value=Foobar
		public static void UpdateProperty(string name, string propertyName, string value)
		{
			Connect();
			ws.Send(string.Format("cmd=CmdUpdateProperty&Name={0}&PropertyName={1}&Value={2}", name, propertyName, value));
		}

		public static void ClearCanvas()
		{
			Connect();
			ws.Send(string.Format("cmd=CmdClearCanvas"));
		}

		public static void DropShape(string shapeName, string name, int x, int y, string text = "")
		{
			Connect();
			ws.Send(string.Format("cmd=CmdDropShape&ShapeName={0}&Name={1}&X={2}&Y={3}&Text={4}", shapeName, x, y, text, name));
		}

		public static void DropShape(string shapeName, string name, Rectangle r, string text = "")
		{
			Connect();
			ws.Send(string.Format("cmd=CmdDropShape&ShapeName={0}&Name={1}&X={2}&Y={3}&Width={4}&Height={5}&Text={6}", shapeName, name, r.X, r.Y, r.Width, r.Height, text));
		}

		public static void DropShape(string shapeName, string name, Rectangle r, Color fillColor, string text = "")
		{
			Connect();
			ws.Send(string.Format("cmd=CmdDropShape&ShapeName={0}&Name={1}&X={2}&Y={3}&Width={4}&Height={5}&Text={6}&FillColor={7}", shapeName, name, r.X, r.Y, r.Width, r.Height, text, fillColor.ToHtmlColor('!')));
		}

		public static void DropShape(string shapeName, string name, int x, int y, int w, int h, string text = "")
		{
			Connect();
			ws.Send(string.Format("cmd=CmdDropShape&ShapeName={0}&Name={1}&X={2}&Y={3}&Width={4}&Height={5}&Text={6}", shapeName, name, x, y, w, h, text));
		}

		public static void DropConnector(string shapeName, string name, int x1, int y1, int x2, int y2)
		{
			Connect();
			ws.Send(string.Format("cmd=CmdDropConnector&ConnectorName={0}&Name={1}&X1={2}&Y1={3}&X2={4}&Y2={5}", shapeName, name, x1, y1, x2, y2));
		}

		public static void DropConnector(string shapeName, string name, int x1, int y1, int x2, int y2, Color borderColor)
		{
			Connect();
			ws.Send(string.Format("cmd=CmdDropConnector&ConnectorName={0}&Name={1}&X1={2}&Y1={3}&X2={4}&Y2={5}&BorderColor={6}", shapeName, name, x1, y1, x2, y2, borderColor.ToHtmlColor('!')));
		}

		private static void Connect()
		{
			if (ws == null || !ws.IsAlive)
			{
				// ws = new WebSocket("ws://192.168.1.165:1100/flowsharp", new MyListener());
				string localip = GetLocalHostIPs()[0].ToString();
				ws = new WebSocket("ws://" + localip + ":1100/flowsharp", new MyListener());

				ws.OnMessage += (sender, e) =>
				{
					response = e.Data;
				};

				ws.Connect();
			}
		}

		private static List<IPAddress> GetLocalHostIPs()
		{
			IPHostEntry host;
			host = Dns.GetHostEntry(Dns.GetHostName());
			List<IPAddress> ret = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();

			return ret;
		}
	}
}