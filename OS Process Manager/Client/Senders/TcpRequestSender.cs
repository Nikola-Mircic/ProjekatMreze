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
            string processName = "";
            
            try
            {
                Console.WriteLine("Enter process name:");
                processName = Console.ReadLine(); //promeniti da salje procese
                Process process = new Process(processName);

                byte[] msg = Process.Serialize(process);

                int bytesSent = clientSocket.Send(msg);

                int bytesReceived = clientSocket.Receive(msg);

                if(Encoding.UTF8.GetString(msg, 0, bytesReceived) != "SUCCESS")
                {
                    Console.WriteLine($"Process failed to start:\n\t{process}");
                    Client.Processes.Add(process);
                }
            }
            catch
            {
                Console.WriteLine("Failure");
            }
        }
    }
}
