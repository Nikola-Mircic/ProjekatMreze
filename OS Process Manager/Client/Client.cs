using Client.Senders;
using ProcessOS;
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
        private static IPAddress ipAddress = IPAddress.Loopback; // Svuda koristim Loopback posto rade na istoj masini
        private static int serverPort = 8000;

        private static void readInput(ref ClientBuilder client)
        {
            int opt = -1;
            do
            {
                if (ClientBuilder.ForceStop)
                {
                    Console.Clear();
                    break;
                }
                ShowMenu();
                if (!int.TryParse(Console.ReadLine(), out opt))
                {
                    opt = -1;
                }
                switch (opt)
                {
                    case 1:
                        string processName = "";
                        Console.WriteLine("Enter process name:");
                        processName = Console.ReadLine();
                        Process process = new Process(processName);

                        client.Add(process);

                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case 0:
                        break;
                }

            } while (opt != 0);

            client.senderRunning = false;
            client.mrse.Set();
            Console.Clear();
        }

        private static void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("Choose option: ");
            Console.WriteLine("1. Send process");
            Console.WriteLine("0. Quit");
        }

        static void Main(string[] args)
        {
            ClientBuilder client = new ClientBuilder(ipAddress, serverPort);
            bool successful = client.connectToTCPSocket();
            if (!successful)
            {
                Console.WriteLine("[Client] failed to connect!");
                Console.WriteLine("Press any key to quit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("[UDP socket] closing...");
            client.udpRequestSender.Close();

            Thread sender = new Thread(() => { client.SendProcessQueue(); });
            client.senderRunning = true;
            sender.Start();

            readInput(ref client);

            if (!ClientBuilder.ForceStop)
            {
                Console.WriteLine("[TCP socket] sending remaining processes...\nPlease wait!");
                sender.Join();
            }

            Console.WriteLine("[TCP socket] closing...");
            client.tcpRequestSender.Close();
            Console.WriteLine("[Client] shutting down...\nPress any key to quit...");
            Console.ReadKey();
        }

    }
}
