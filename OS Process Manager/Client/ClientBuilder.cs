using Client.Senders;
using ProcessOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    internal class ClientBuilder
    {
        public UdpRequestSender udpRequestSender { get; private set; }
        public TcpRequestSender tcpRequestSender { get; private set; }

        internal static bool ForceStop { get; set; } = false;
        public static List<Process> toSend { get; private set; }
        public bool senderRunning { get; set; }
        public ManualResetEvent mrse { get; set; } = new ManualResetEvent(false);

        public ClientBuilder(IPAddress ipAddress, int serverPort)
        {
            toSend = new List<Process>();

            udpRequestSender = new UdpRequestSender(ipAddress, serverPort);

        }

        public (string, int, bool) GetServerTcpSocket()
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

        public bool connectToTCPSocket()
        {
            (string ip, int port, bool successful) = GetServerTcpSocket();
            if (!successful)
            {
                Console.WriteLine("Connection failed!\nDisconnecting...");
                return false;
            }
            //uspostavljanje tcp konekcije sa serverom
            tcpRequestSender = new TcpRequestSender(IPAddress.Parse(ip), port);
            successful = tcpRequestSender.RequestConnection();
            if (!successful)
            {
                Console.WriteLine("Connection failed!\nDisconnecting...");
                return false;
            }
            return true;
        }

        public void Add(Process process)
        {
            lock (toSend)
            {
                toSend.Add(process);
            }
            mrse.Set();
        }

        public void SendProcessQueue()
        {
            while (!ForceStop)
            {
                mrse.WaitOne();// Ceka da se nesto ubaci u listu

                lock (toSend)
                {
                    if (toSend.Count == 0)
                    {
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
                    }
                }
                Thread.Sleep(200);
            }
        }
    }
}
