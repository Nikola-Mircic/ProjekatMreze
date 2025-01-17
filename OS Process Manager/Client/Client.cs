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
        private IPAddress iPAddress = IPAddress.Loopback; // Svuda koristim Loopback posto rade na istoj masini
        private int serverPort = 8000;

        private UdpRequestSender udpRequestSender;
        private TcpRequestSender tcpRequestSender;

        private static List<Process> toSend;
        private Thread sender;
        private bool senderRunning;
        private ManualResetEvent mrse;

        private Client()
        {
            toSend = new List<Process>();

            udpRequestSender = new UdpRequestSender(iPAddress, serverPort);

            connectToTCPSocket();

            sender = new Thread(() => { SendProcessQueue(); });
            mrse = new ManualResetEvent(false);
            senderRunning = true;
            sender.Start();
        }

        private (string, int, bool) GetServerTcpSocket()
        {
            /*
                slanje zahteva za konekciju
                ukoliko se konekcija ne uspostavi u maxAttempts pokusaja, prekida se rad
            */
            string ip = string.Empty;
            int port = -1;
            bool successful = false;

            int maxAttempts = 3;

            do
            {
                (ip, port, successful) = udpRequestSender.RequestConnection();

                if (udpRequestSender.ConnectionAttempts >= maxAttempts)
                {
                    Console.WriteLine("Connection failed!\nDisconnecting...");
                    Console.ReadKey();
                    return ("", 0, false);
                }
                if (!successful)
                {
                    Thread.Sleep(1000);
                }

            } while (!successful);

            return (ip, port, successful);
        }

        public void connectToTCPSocket()
        {
            (string ip, int port, bool successful) = GetServerTcpSocket();

            //uspostavljanje tcp konekcije sa serverom
            tcpRequestSender = new TcpRequestSender(IPAddress.Parse(ip), port);
            successful = tcpRequestSender.RequestConnection();
            if (!successful)
            {
                Console.WriteLine("Connection failed!\nDisconnecting...");
                return;
            }
        }

        public void readInput()
        {
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
                        string processName = "";
                        Console.WriteLine("Enter process name:");
                        processName = Console.ReadLine(); //promeniti da salje procese
                        Process process = new Process(processName);

                        lock (toSend)
                        {
                            toSend.Add(process);
                        }

                        mrse.Set();

                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case 0:
                        break;
                }

            } while (opt != 0);

            Console.WriteLine("Slanje preostalih procesa...");
            senderRunning = false;
            mrse.Set();
            sender.Join();

            Console.WriteLine("Gasenje uticnica...");
            tcpRequestSender.Close();
            udpRequestSender.Close();

            Console.WriteLine("Klijent uspesno ugasen.");
        }

        private static void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("Choose option: ");
            Console.WriteLine("1. Request information");
            Console.WriteLine("2. Send process");
            Console.WriteLine("0. Quit");
        }

        static void Main(string[] args)
        {
            Client client = new Client();
            client.readInput();
        }

        private void SendProcessQueue()
        {
            while (true) 
            {
                mrse.WaitOne();// Ceka da se nesto ubaci u listu

                lock (toSend)
                {
                    if (toSend.Count == 0) {
                        if (!senderRunning)
                            return;

                        mrse.Reset(); /// Poslati su svi procesi, ulazi se u stanje cekanja
                        continue;
                    }

                    // Provera da li je proces uspesno poslat
                    if (tcpRequestSender.SendProcess(toSend.First()))
                    {
                        // Ako je proces uspesno poslat, uklanja se iz liste
                        toSend.RemoveAt(0);
                        Thread.Sleep(1000);
                    }
                }
            }
        }
    }
}
