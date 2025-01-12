using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client.Senders
{
    internal class TcpRequestSender
    {
        Socket clientSocket = null;

        private IPAddress IPAddress;
        private int serverPort;

        public int ConnectionAttempts { get; private set; }

        public TcpRequestSender(IPAddress IPAddress, int serverPort)
        {
            this.IPAddress = IPAddress;
            this.serverPort = serverPort;
        }
    }
}
