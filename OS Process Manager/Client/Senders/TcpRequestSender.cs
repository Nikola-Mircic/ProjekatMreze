using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ProcessOS;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Client.Senders
{
    internal class TcpRequestSender
    {
        Socket clientSocket = null;

        private readonly IPAddress IPAddress;
        private readonly int serverPort;

        private Queue<Process> toSend;

        public TcpRequestSender(IPAddress IPAddress, int serverPort)
        {
            this.IPAddress = IPAddress;
            this.serverPort = serverPort;

            this.toSend = new Queue<Process>();
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

        public bool SendProcess(Process process)
        {
            try
            {
                byte[] msg = Process.Serialize(process);

                int bytesSent = clientSocket.Send(msg);

                int bytesReceived = clientSocket.Receive(msg);

                if(Encoding.UTF8.GetString(msg, 0, bytesReceived) != "SUCCESS")
                {
                    Console.WriteLine($"Process failed to start:\n\t{process}");
                    return false;
                }
            }
            catch
            {
                Console.WriteLine("Failure");
                return false;
            }
            Console.WriteLine("Process finished!");
            return true;
        }
    }
}
