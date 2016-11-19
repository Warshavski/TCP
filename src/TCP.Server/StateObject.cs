using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCP.Server
{
    /// <summary>
    ///     State object for reading client data asynchronously
    /// </summary>
    public class StateObject
    {
        // Client socket
        public Socket WorkSocket { get; set; }

        // Size of receive buffer
        public const int BufferSize = 1024;

        // Receive buffer
        public byte[] Buffer = new byte[BufferSize];

        // Received data string 
        public StringBuilder Sb = new StringBuilder();
    }
}
