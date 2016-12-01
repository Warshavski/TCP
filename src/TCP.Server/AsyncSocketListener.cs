using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCP.Server
{
    public class AsyncSocketListener
    {
        // Thread signal
        public static ManualResetEvent AllDone = new ManualResetEvent(false);

        public AsyncSocketListener() { }

        public static void StartListening()
        {
            // Data buffer for incoming data.
            var dataBuffer = new byte[1024];

            // Establish the local endpoint for the socket
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 110000);

            // Create a TCP/IP socket
            var listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the endpoint and listen for incoming connections
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state
                    AllDone.Reset();

                    listener.BeginAccept(new AsyncCallback(OnAcceptCallback), listener);

                    // Wait until a connection is made before continuing
                    AllDone.WaitOne();

                }
            }
            catch(SocketException ex)
            {
                // write exception
            }
        }

        public static void OnAcceptCallback(IAsyncResult asyncResult)
        {
            // Signal the main thread to continue
            AllDone.Set();

            // Get the socket that handles the client request
            var listener = (Socket)asyncResult.AsyncState;
            Socket handler = listener.EndAccept(asyncResult);

            // Create the state object
            var state = new StateObject();
            state.WorkSocket = handler;
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult asyncResult)
        {
            var content = String.Empty;

            // Retrieve the state object and the handler soket
            // from asynchronous state object
            var state = (StateObject)asyncResult.AsyncState;
            Socket handler = state.WorkSocket;

            // Read data from the client socket
            int bytesRead = handler.EndReceive(asyncResult);

            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.
                state.Sb.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read more data
                content = state.Sb.ToString();
                if (content.IndexOf("<EOF>")  > -1)
                {
                    // All the data has been read from the client.
                    // Display it on the console
                    //***Console.WriteLine("Read bytes from socket. \n Data {1}, content.Lenght, content);

                    // Echo the data back to the client
                    Send(handler, content);
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                // Retrieve the socket from the state object
                Socket handler = (Socket)asyncResult.AsyncState;

                // Complete sending the data to the remote device
                int bytesSent = handler.EndSend(asyncResult);

                //*** Console.WriteLine("Sent {0} bytes to the client", byteSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch(SocketException ex)
            {
                //*** Console.WriteLine(ex.ToString());
            }
        }

        public 
    }
}
