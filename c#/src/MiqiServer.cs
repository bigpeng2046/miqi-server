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
        private readonly WebSocketServer m_server;

        public MiqiServer(IPAddress address, int port)
        {
            m_server = new WebSocketServer(address, port);
            m_server.OnClientConnected += OnClientConnected;
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
            client.Send(data);
        }
    }
}
