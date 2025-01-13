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

        private readonly IPAddress IPAddress;
        private readonly int serverPort;

        public int ConnectionAttempts { get; private set; } //nije implementirano

        public TcpRequestSender(IPAddress IPAddress, int serverPort)
        {
            this.IPAddress = IPAddress;
            this.serverPort = serverPort;
        }

        public void Close()
        {
            if (clientSocket != null)
            {
                clientSocket.Close();
            }
        }

        public bool RequestConnection()
        {
            if (clientSocket == null)
            {
                try
                {
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                catch(SocketException ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress, serverPort);
                clientSocket.Connect(ipEndPoint);
                return true;
            }
            catch(SocketException ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public void SendProcess()
        {
            string msg = "";
            
            try
            {
                msg = Console.ReadLine(); //promeniti da salje procese
                int bytesSent = clientSocket.Send(Encoding.UTF8.GetBytes(msg));
            }
            catch
            {
                Console.WriteLine("Failure");
            }
        }
    }
}
