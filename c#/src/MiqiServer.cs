using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using Miqi;

namespace Miqi.Net {

    public class MiqiServer
    {
		private readonly int m_port;
		private readonly string m_ipAddresses;
        private readonly WebSocketServer m_server;

        private MainForm m_logForm;
		private MiqiMessageHandlerCollection m_msgHandlers;
		
        public MiqiServer(MainForm logForm, int port)
        {
			m_port = port;
            m_server = new WebSocketServer(IPAddress.Any, port);
            m_server.OnClientConnected += OnClientConnected;
			m_ipAddresses = GetLocalIPAddresses();
			
            m_logForm = logForm;
			m_msgHandlers = new MiqiMessageHandlerCollection();
			m_msgHandlers.AddHandler(MiqiMessage.GET_SERVER_INFO, HandleGetServerInfoMessage);
			m_msgHandlers.AddHandler(MiqiMessage.SET_CREDENTIAL, HandleSetCredentialMessage);

            m_logForm.Log("Listening on {0}", m_ipAddresses);
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

            m_logForm.Log("Client {0} Connected...", client.Id);
        }

        private void OnClientDisconnected(WebSocketClient client)
        {
            client.ReceivedTextualData -= OnReceivedTextualData;
            client.Disconnected -= OnClientDisconnected;

            m_logForm.Log("Client {0} Disconnected...", client.Id);
        }

        private void OnReceivedTextualData(WebSocketClient client, string data)
        {
            m_logForm.Log("Client {0} Received message...", client.Id);
			// Console.WriteLine("Client {0} Received message... {1}", client.Id, data);
			
			try
			{
				MiqiMessage message = MiqiMessage.BuildFromString(data);
				m_msgHandlers.HandleMessage(client, message);
			}
			catch (Exception ex)
			{
                m_logForm.Log("OnReceivedTextualData: {0}", ex.Message);
				client.Disconnect();
			}			
        }
		
        private static string GetLocalIPAddresses()
        {
			try
			{
				string hostName = Dns.GetHostName();
				IPHostEntry ipEntry = Dns.GetHostEntry(hostName);

				StringBuilder builder = new StringBuilder();
				
				foreach (IPAddress ip in ipEntry.AddressList)
				{
					//IPV4
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						builder.AppendFormat("{0},", ip.ToString());
					}
				}
				
				if (builder.Length > 0)
				{
					builder.Remove(builder.Length - 1, 1);
				}
				
				return builder.ToString();
			}
			catch (Exception)
			{
				return "";
			}
        }
		
		private void HandleGetServerInfoMessage(WebSocketClient client, MiqiMessage message)
		{
			MiqiMessage resp = MiqiMessage.BuildGetSeverInfoResponse(m_ipAddresses, m_port, client.Id);
			client.Send(resp.ToString());
		}
		
		private void HandleSetCredentialMessage(WebSocketClient client, MiqiMessage message)
		{
			string reqClientId = message.GetHeader("ClientId");
			WebSocketClient reqClient = m_server.GetClientById(reqClientId);
			if (reqClient != null) {
				reqClient.Send(MiqiMessage.BuildSetCredential(message).ToString());
				client.Disconnect();
			} else {
                m_logForm.Log("Cannot find the client: {0}", reqClientId);
			}			
		}
    }
}
