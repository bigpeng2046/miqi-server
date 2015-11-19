using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Miqi.Net
{
    public class WebSocketServer
    {
        private bool m_isStarted;
        private readonly TcpListener m_tcpListener;
        private readonly Dictionary<Guid, WebSocketClient> m_clients = new Dictionary<Guid, WebSocketClient>(); 
        private readonly object m_sync = new object();

        public event Action<WebSocketClient> OnClientConnected = delegate { };

        public WebSocketServer(IPAddress address, int port)
        {
            m_tcpListener = new TcpListener(address, port);
        }

        public void Start()
        {
            if (m_isStarted)
                return;

            lock (m_sync)
            {
                if (m_isStarted)
                    return;

                m_isStarted = true;
                m_tcpListener.Start();
                m_tcpListener.BeginAcceptTcpClient(OnAcceptClient, null);
            }
        }

        public void Stop()
        {
            if (!m_isStarted)
                return;

            lock(m_sync)
            {
                if (!m_isStarted)
                    return;

                m_isStarted = false;
                m_tcpListener.Stop();

                foreach (WebSocketClient client in m_clients.Values)
                {
                    client.Disconnect();
                }
            }
        }

        private void OnAcceptClient(IAsyncResult asyncResult)
        {
            if (!m_isStarted)
                return;

            TcpClient client = m_tcpListener.EndAcceptTcpClient(asyncResult);
            ReceiveClientHandshake(client);

            m_tcpListener.BeginAcceptTcpClient(OnAcceptClient, null);
        }

        private void ReceiveClientHandshake(TcpClient client)
        {
            byte[] buffer = new byte[1024];
            SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();

            socketAsyncEventArgs.UserToken = client;
            socketAsyncEventArgs.Completed += OnHandshakeReceived;
            socketAsyncEventArgs.SetBuffer(buffer, 0, buffer.Length);

            bool isAsync = client.Client.ReceiveAsync(socketAsyncEventArgs);
            if (!isAsync)
                OnHandshakeReceived(client.Client, socketAsyncEventArgs);
        }

        private void OnHandshakeReceived(object sender, SocketAsyncEventArgs e)
        {
            TcpClient client = (TcpClient) e.UserToken;

            int numberOfBytesReceived = e.SocketError != SocketError.Success ? 0 : e.BytesTransferred;
            if (numberOfBytesReceived <= 0)
            {
                client.Client.Shutdown(SocketShutdown.Both);
                client.Close();
                return;
            }

            // Note: We're working under the assumption that the entire handshake will arrive in one frame
            string data = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);
            string handshakeString = OpeningHandshakeHandler.CreateServerHandshake(data);
            if (String.IsNullOrEmpty(handshakeString))
            {
                client.Client.Shutdown(SocketShutdown.Both);
                client.Close();
                return;
            }

            byte[] handshakeBytes = Encoding.UTF8.GetBytes(handshakeString);
            SendHandshake(client, handshakeBytes);
        }

        private void SendHandshake(TcpClient client, byte[] handshake)
        {
            SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();

            sendEventArgs.UserToken = client;
            sendEventArgs.SetBuffer(handshake, 0, handshake.Length);
            sendEventArgs.Completed += OnHandshakeSendCompleted;

            client.Client.SendAsync(sendEventArgs);
        }

        private void OnHandshakeSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            TcpClient client = (TcpClient)e.UserToken;

            WebSocketClient wsClient = new WebSocketClient(Guid.NewGuid(), client);
            wsClient.Disconnected += OnClientDisconnected;

            m_clients.Add(wsClient.Id, wsClient);
            OnClientConnected(wsClient);
        }

        private void OnClientDisconnected(WebSocketClient client)
        {
            client.Disconnected -= OnClientDisconnected;

            m_clients.Remove(client.Id);
        }
    }
}