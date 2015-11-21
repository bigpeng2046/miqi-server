using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace Miqi.Net {

    public class MiqiServer
    {
		private readonly int m_port;
		private readonly IPAddress[] m_ipAddresses;
        private readonly WebSocketServer m_server;

		private MiqiMessageHandlerCollection m_msgHandlers;
		
        public MiqiServer(int port)
        {
			m_port = port;
            m_server = new WebSocketServer(IPAddress.Any, port);
            m_server.OnClientConnected += OnClientConnected;
			m_ipAddresses = GetLocalIPAddresses();
			
			m_msgHandlers = new MiqiMessageHandlerCollection();
			m_msgHandlers.AddHandler(MiqiMessage.GET_SERVER_INFO, HandleGetServerInfoMessage);
			m_msgHandlers.AddHandler(MiqiMessage.SET_CREDENTIAL, HandleSetCredentialMessage);
        }

        public void Start()
        {
            m_server.Start();
        }

        public void Stop()
        {
            m_server.Stop();
        }

        private void OnClientConnected(WebSocketClient client)
        {
            client.ReceivedTextualData += OnReceivedTextualData;
            client.Disconnected += OnClientDisconnected;
            client.StartReceiving();

            Console.WriteLine("Client {0} Connected...", client.Id);
        }

        private void OnClientDisconnected(WebSocketClient client)
        {
            client.ReceivedTextualData -= OnReceivedTextualData;
            client.Disconnected -= OnClientDisconnected;

            Console.WriteLine("Client {0} Disconnected...", client.Id);
        }

        private void OnReceivedTextualData(WebSocketClient client, string data)
        {
            Console.WriteLine("Client {0} Received Message: {1}", client.Id, data);
			
			try
			{
				MiqiMessage message = MiqiMessage.BuildFromString(data);
				m_msgHandlers.HandleMessage(client, message);
			}
			catch (Exception ex)
			{
				Console.WriteLine("OnReceivedTextualData: {0}", ex.Message);
				client.Disconnect();
			}			
        }
		
        private static IPAddress[] GetLocalIPAddresses()
        {
			try
			{
				string hostName = Dns.GetHostName();
				IPHostEntry ipEntry = Dns.GetHostEntry(hostName);

				foreach (IPAddress ip in ipEntry.AddressList)
				{
					Console.WriteLine("{0}", ip);
					//IPV4
					// if (ip.AddressFamily == AddressFamily.InterNetwork)
					//	return ip;
				}
				
				return ipEntry.AddressList;
			}
			catch (Exception)
			{
				return null;
			}
        }
		
		private void HandleGetServerInfoMessage(WebSocketClient client, MiqiMessage message)
		{
			MiqiMessage resp = MiqiMessage.BuildGetSeverInfoResponse(m_ipAddresses[0].ToString(), m_port, client.Id);
			client.Send(resp.ToString());
		}
		
		private void HandleSetCredentialMessage(WebSocketClient client, MiqiMessage message)
		{
			string reqClientId = message.GetHeader("ClientId");
			WebSocketClient reqClient = m_server.GetClientById(reqClientId);
			if (reqClient != null) {
				reqClient.Send("user:{0}, password:{1}");
				client.Disconnect();
			} else {
				Console.WriteLine("Cannot find the client: {0}", reqClientId);
			}			
		}
    }
}
