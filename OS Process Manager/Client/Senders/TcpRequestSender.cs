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
        private Socket clientSocket = null;

        private IPEndPoint endPoint = null;

        private Queue<Process> toSend;

        public TcpRequestSender(IPAddress IPAddress, int serverPort)
        {
            endPoint = new IPEndPoint(IPAddress, serverPort);

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
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(endPoint);
                return true;
            }
            catch(SocketException ex)
            {
                Console.WriteLine($"[Client TCP socket] failed to connect: {ex.SocketErrorCode}");
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
                    return false;
                }
            }
            catch(SocketException ex)
            {
                Console.Clear();
                Console.WriteLine($"[Client TCP socket]: {ex.SocketErrorCode}");
                Console.WriteLine("Press enter to continue...");
                toSend.Clear();
                ClientBuilder.ForceStop = true;
                
                return false;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[Client TCP socket] error:\n{ex.Message}");
                return false;
            }
            return true;
        }
    }
}
