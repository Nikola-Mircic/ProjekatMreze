using Client.Senders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class Client
    {
        static void Main(string[] args)
        {
            Socket clientUdpSocket = null;
            IPAddress iPAddress = IPAddress.Loopback; // Svuda koristim Loopback posto rade na istoj masini
            int clientPort = 9000;
            int maxAttempts = 3;

            try
            {
                // Uticnica za klijenta
                clientUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint client = new IPEndPoint(iPAddress, clientPort);
                clientUdpSocket.Bind(client);
            }
            catch(SocketException ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
                return;
            }

            /*
                slanje zahteva za konekciju
                ukoliko se konekcija ne uspostavi u maxAttempts pokusaja, prekida se rad
            */
            ConnectionRequestSender connectionRequestSender = new ConnectionRequestSender(IPAddress.Loopback, 8000);

            string ip = string.Empty;
            int port = -1;
            bool successful = false;

            do
            {
                (ip, port, successful) = connectionRequestSender.RequestConnection(clientUdpSocket);

                if (connectionRequestSender.ConnectionAttempts >= maxAttempts)
                {
                    Console.WriteLine("Connection failed!\nDisconnecting...");
                    Console.ReadKey();
                    return;
                }
                if (!successful)
                {
                    Thread.Sleep(1000);
                }

            } while (!successful);


            //uspostaviti tcp konekciju sa dobijenim informacijama (TODO)

        }
    }
}
