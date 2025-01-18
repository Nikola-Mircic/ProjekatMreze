using Server.Managers;
using Server.Receivers;
using Server.Schedulers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientProcess = System.Diagnostics.Process;

namespace Server
{
    public class Server
    {
        public static readonly int CLIENT_CONN_PORT = 8000;
        public static readonly int PROCESS_PORT = 8001;
        public static readonly int MAX_CLIENTS = 5;
        public static readonly int START_CLIENTS = 1;

        static void Main(string[] args)
        {
            ClientReceiver receiver = new ClientReceiver(CLIENT_CONN_PORT);
            receiver.ProcessReceiver = new IPEndPoint(IPAddress.Loopback, PROCESS_PORT);

            (var schedulingMode, var success) = ChooseScheduling();
            Console.Clear();
            if (!success)
            {
                Console.WriteLine("[Server] shutting down...");
                return;
            }
            Console.WriteLine($"[Scheduling]: {schedulingMode}");

            //udp listener
            Thread udpThread = new Thread(() => receiver.Start());
            udpThread.Start();
            
            ProcessReceiver tcpProcessReceiver = new ProcessReceiver(IPAddress.Loopback, PROCESS_PORT, MAX_CLIENTS);

            //tcp listener
            Thread tcpThread = new Thread(() => tcpProcessReceiver.Start());
            tcpThread.Start();

            StartClient(START_CLIENTS);

            Console.WriteLine("[Receiver] Started!");
            Console.ReadLine();

            tcpProcessReceiver.Stop(tcpThread);
            receiver.Stop();

            Manager.Get().ServerInfo();

        }

        private static void StartClient(int clients)
        {
            for (int i = 0; i < clients; i++)
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..", "Client\\bin\\Debug\\Client");
                ClientProcess client = new ClientProcess(); //System.Diagnostics.Process
                client.StartInfo.FileName = path;
                client.StartInfo.Arguments = $"{i + 1}";
                Console.WriteLine($"[Client {i + 1}] started");
                client.Start();
            }
        }

        private static void ScheduleOptions()
        {
            Console.Clear();
            Console.WriteLine("Choose option: ");
            Console.WriteLine("1. Round Robin Scheduling");
            Console.WriteLine("2. Shortest First Scheduling");
            Console.WriteLine("0. Quit");
        }

        private static (string, bool) ChooseScheduling()
        {
            int scheduleOpt = -1;
            do
            {
                ScheduleOptions();
                if (!int.TryParse(Console.ReadLine(), out scheduleOpt))
                {
                    scheduleOpt = -1;
                }

                switch (scheduleOpt)
                {
                    case 1:
                        Manager.Init(new RoundRobinScheduler(1));
                        return ("Round Robin Scheduling", true);
                        
                    case 2:
                        Manager.Init(new ShortestFirstScheduler());
                        return ("Shortest First Scheduling", true);;
                    case 0:
                        break;
                }

            } while (scheduleOpt < 0);
            return ("", false);
        }
    }
}
