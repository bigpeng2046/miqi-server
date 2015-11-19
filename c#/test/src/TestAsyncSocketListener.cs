using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;

// State object for reading client data asynchronously
public class StateObject {
    // Client  socket.
    public Socket workSocket = null;
    // Size of receive buffer.
    public const int BufferSize = 1024;
    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];
// Received data string.
    public StringBuilder sb = new StringBuilder();  
}

public class AsynchronousSocketListener {
    // Thread signal.
    public static ManualResetEvent allDone = new ManualResetEvent(false);

    public AsynchronousSocketListener() {
    }

    public static void StartListening() {
        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];

        // Establish the local endpoint for the socket.
        // The DNS name of the computer
        // running the listener is "host.contoso.com".
        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
		/*
		foreach (IPAddress ipAddr in ipHostInfo.AddressList) {
			Console.WriteLine("{0}", ipAddr);
		}
		*/
		IPAddress localAddress = IPAddress.Parse("127.0.0.1");
		
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 11000);
		IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 11000);

        // Create a TCP/IP socket.
        Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp );

        // Bind the socket to the local endpoint and listen for incoming connections.
        try {
            listener.Bind(localEndPoint);
            listener.Listen(100);

            while (true) {
                // Set the event to nonsignaled state.
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.
                Console.WriteLine("{0}: Waiting for a connection... : {1}", Thread.CurrentThread.ManagedThreadId, localEndPoint);
                listener.BeginAccept( 
                    new AsyncCallback(AcceptCallback),
                    listener );

                // Wait until a connection is made before continuing.
                allDone.WaitOne();
				Console.WriteLine("{0}: A connection comes.", Thread.CurrentThread.ManagedThreadId);
            }

        } catch (Exception e) {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();
        
    }

    public static void AcceptCallback(IAsyncResult ar) {
        // Signal the main thread to continue.
        allDone.Set();

        // Get the socket that handles the client request.
        Socket listener = (Socket) ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        // Create the state object.
        StateObject state = new StateObject();
        state.workSocket = handler;
        handler.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);
    }

    public static void ReadCallback(IAsyncResult ar) {
        String content = String.Empty;
        
        // Retrieve the state object and the handler socket
        // from the asynchronous state object.
        StateObject state = (StateObject) ar.AsyncState;
        Socket handler = state.workSocket;

        // Read data from the client socket. 
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0) {
            // There  might be more data, so store the data received so far.
            state.sb.Append(Encoding.UTF8.GetString(
                state.buffer, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read 
            // more data.
            content = state.sb.ToString();
			Console.WriteLine("{0}: {1}", Thread.CurrentThread.ManagedThreadId, content);
            if (content.IndexOf("\r\n\r\n") > -1) {
                // All the data has been read from the 
                // client. Display it on the console.
                Console.WriteLine("{0}: Read {1} bytes from socket. \n Data : {2}",
                    Thread.CurrentThread.ManagedThreadId, content.Length, content );
                // Echo the data back to the client.
                Send(handler, GetResponseString(content));
            } else {
                // Not all data received. Get more.
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
            }
        }
    }
    
    private static void Send(Socket handler, String data) {
        // Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.UTF8.GetBytes(data);

        // Begin sending the data to the remote device.
        handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
    }

    private static void SendCallback(IAsyncResult ar) {
        try {
            // Retrieve the socket from the state object.
            Socket handler = (Socket) ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = handler.EndSend(ar);
            Console.WriteLine("{0}: Sent {1} bytes to client.", Thread.CurrentThread.ManagedThreadId, bytesSent);

            // handler.Shutdown(SocketShutdown.Both);
            // handler.Close();

        } catch (Exception e) {
            Console.WriteLine(e.ToString());
        }
    }

	private static string GetResponseString(string handshake) {
		string[] handshakeLines = handshake.Split(new string[] { Environment.NewLine },
				System.StringSplitOptions.RemoveEmptyEntries);
		string acceptKey = "";
		foreach (string line in handshakeLines)	{
			if (line.Contains("Sec-WebSocket-Key:")) {
				acceptKey = ComputeWebSocketHandshakeSecurityHash09(line.Substring(line.IndexOf(":") + 2));
			}
		}

		return string.Format(RESPONSE_TEMPLATE, acceptKey);
	}

	private static String ComputeWebSocketHandshakeSecurityHash09(string secWebSocketKey)
	{
		const String MagicKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
		
		String secWebSocketAccept = String.Empty;
		// 1. Combine the request Sec-WebSocket-Key with magic key.
		String ret = secWebSocketKey + MagicKEY;
		
		// 2. Compute the SHA1 hash
		SHA1 sha = new SHA1CryptoServiceProvider();
		byte[] sha1Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ret));
		
		// 3. Base64 encode the hash
		secWebSocketAccept = Convert.ToBase64String(sha1Hash);
		return secWebSocketAccept;
	}
	
	private static string RESPONSE_TEMPLATE = "HTTP/1.1 101 Switching Protocols" + Environment.NewLine
            + "Upgrade: WebSocket" + Environment.NewLine
            + "Connection: Upgrade" + Environment.NewLine
            + "Sec-WebSocket-Accept: {0}" + Environment.NewLine + Environment.NewLine;
	
    public static int Main(String[] args) {
        StartListening();
        return 0;
    }
}
