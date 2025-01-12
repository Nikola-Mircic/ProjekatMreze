using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client.Senders
{
    internal class ConnectionRequestSender
    {
        private IPAddress IPAddress;
        private int serverPort;

        public int ConnectionAttempts { get; private set; }

        public ConnectionRequestSender(IPAddress IPAddress, int serverPort)
        {
            this.IPAddress = IPAddress;
            this.serverPort = serverPort;
        }

        public (string ip, int port, bool success) RequestConnection(Socket socket)
        {
            // Adresa serverske UDP uticnice za prijavljivanje
            IPEndPoint server = new IPEndPoint(IPAddress, serverPort);

            byte[] msg = Encoding.UTF8.GetBytes("CONNECT");
            try
            {
                socket.SendTo(msg, server);
                Console.WriteLine("Message sent!");


                byte[] targetEndPoint = new byte[1024]; // Buffer za cuvanje adrese
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // Promenjiva za cuvanje adrese posiljaoca
                int bytesReceived = socket.ReceiveFrom(targetEndPoint, SocketFlags.None, ref remoteEndPoint);

                // Ako je posaljilac server -> ispisi adresu iz odgovora
                if (remoteEndPoint.Equals(server))
                {
                    string[] reply = Encoding.UTF8.GetString(targetEndPoint).Split(':');
                    Console.WriteLine(Encoding.UTF8.GetString(targetEndPoint));
                    return (reply[0], int.Parse(reply[1]), true);
                }

                ConnectionAttempts++;
                return ("", -1, false);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ConnectionAttempts++;
                return ("", -1, false);
            }
        }
    }
}
