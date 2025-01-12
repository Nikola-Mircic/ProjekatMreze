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
            IPAddress iPAddress = IPAddress.Loopback; // Svuda koristim Loopback posto rade na istoj masini
            int serverPort = 8000;

            int maxAttempts = 3;

            /*
                slanje zahteva za konekciju
                ukoliko se konekcija ne uspostavi u maxAttempts pokusaja, prekida se rad
            */
            UdpRequestSender udpRequestSender = new UdpRequestSender(iPAddress, serverPort);

            string ip = string.Empty;
            int port = -1;
            bool successful = false;

            do
            {
                (ip, port, successful) = udpRequestSender.RequestConnection();

                if (udpRequestSender.ConnectionAttempts >= maxAttempts)
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
            TcpRequestSender tcpRequestSender = new TcpRequestSender(IPAddress.Parse(ip), port);

        }
    }
}
