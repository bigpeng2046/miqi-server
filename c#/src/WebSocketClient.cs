using System;
using System.Text;
using System.Net.Sockets;

namespace Miqi.Net
{
    public class WebSocketClient
    {
        private readonly TcpClient m_tcpClient;
        private bool m_isConnected;

		private string id;
        public string Id {
			get { return this.id; } 
			set { this.id = value; }
		}

		public delegate void ReceivedTextualDataHandler(WebSocketClient client, string message);
		public delegate void ReceivedBinaryDataHandler(WebSocketClient client, byte[] data);
		public delegate void DisconnectedHandler(WebSocketClient client);
		
		public event ReceivedTextualDataHandler ReceivedTextualData;
		public event ReceivedBinaryDataHandler ReceivedBinaryData;
		public event DisconnectedHandler Disconnected;
		
        public WebSocketClient(string id, TcpClient tcpClient)
        {
            m_tcpClient = tcpClient;
            this.id = id;
            m_isConnected = true;
        }

        public void StartReceiving()
        {
            byte[] buffer = new byte[1024];
            SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();

            socketAsyncEventArgs.Completed += OnDataReceived;
            socketAsyncEventArgs.SetBuffer(buffer, 0, buffer.Length);

			if (m_tcpClient.Client != null) {
				bool isAsync = m_tcpClient.Client.ReceiveAsync(socketAsyncEventArgs);
				if (!isAsync)
					OnDataReceived(m_tcpClient, socketAsyncEventArgs);
			}
        }

        private void OnDataReceived(object sender, SocketAsyncEventArgs e)
        {
            if (!m_isConnected)
                return;

            int numberOfBytesReceived = e.SocketError != SocketError.Success ? 0 : e.BytesTransferred;
            if (numberOfBytesReceived <= 0)
            {
                Disconnect();
                return;
            }

            if (HandleFrame(e))
                StartReceiving();
        }

        private bool HandleFrame(SocketAsyncEventArgs args)
        {
            Frame frame = Frame.FromBuffer(args.Buffer);

            if (frame.Opcode == Frame.Opcodes.Close)
            {
                Disconnect();
                return false;
            }

            // Note: No support for fragmented messages
            if (frame.Opcode == Frame.Opcodes.Binary)
                ReceivedBinaryData(this, frame.UnmaskedPayload);
            else if (frame.Opcode == Frame.Opcodes.Text)
            {
                string textContent = Encoding.UTF8.GetString(frame.UnmaskedPayload, 0, (int)frame.PayloadLength);
                ReceivedTextualData(this, textContent);
            }

            return true;
        }

        public void Send(byte[] data)
        {
            Frame frame = new Frame(Frame.Opcodes.Binary, data, true);
            Send(frame);
        }

        public void Send(string data)
        {
            Frame frame = new Frame(Frame.Opcodes.Text, Encoding.UTF8.GetBytes(data), true);
            Send(frame);
        }

        private void Send(Frame frame)
        {
            if (!m_isConnected)
                return;

            byte[] buffer = frame.ToBuffer();

            SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
            sendEventArgs.SetBuffer(buffer, 0, buffer.Length);

            m_tcpClient.Client.SendAsync(sendEventArgs);
        }

        public void Disconnect()
        {
            Frame closingFrame = Frame.CreateClosingFrame();
            Send(closingFrame);

            m_isConnected = false;

            m_tcpClient.Client.Shutdown(SocketShutdown.Both);
            m_tcpClient.Close();

            Disconnected(this);
        }
    }
}