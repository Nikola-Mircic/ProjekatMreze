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

            //uspostavljanje tcp konekcije sa serverom
            TcpRequestSender tcpRequestSender = new TcpRequestSender(IPAddress.Parse(ip), port);
            successful = tcpRequestSender.RequestConnection();
            if (!successful)
            {
                Console.WriteLine("Connection failed!\nDisconnecting...");
                return;
            }

            int opt = -1;
            do
            {
                ShowMenu();
                if (!int.TryParse(Console.ReadLine(), out opt))
                {
                    opt = -1;
                }
                switch (opt)
                {
                    case 1:
                        udpRequestSender.RequestInformation(); //nije implementirano
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case 2:
                        tcpRequestSender.SendProcess(); //trenutno radi sa obicnim porukama
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case 0:
                        break;
                }

            } while (opt != 0);

            tcpRequestSender.Close();
            udpRequestSender.Close();
        }

        private static void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("Choose option: ");
            Console.WriteLine("1. Request information");
            Console.WriteLine("2. Send process");
            Console.WriteLine("0. Quit");
        }
    }
}
