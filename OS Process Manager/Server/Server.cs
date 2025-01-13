using Server.Receivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class Server
    {
        public static readonly int CLIENT_CONN_PORT = 8000;
        public static readonly int PROCESS_PORT = 8001;

        static void Main(string[] args)
        {
            ClientReceiver receiver = new ClientReceiver(CLIENT_CONN_PORT);
            receiver.ProcessReceiver = new IPEndPoint(IPAddress.Loopback, PROCESS_PORT);

            //udp listener
            Thread udpThread = new Thread(() => receiver.Start());
            udpThread.Start();
            
            ProcessReceiver tcpProcessReceiver = new ProcessReceiver(IPAddress.Loopback, PROCESS_PORT);
            tcpProcessReceiver.Start();

            //tcp listener
            Thread tcpThread = new Thread(() => tcpProcessReceiver.Connect());
            tcpThread.Start();

            Console.WriteLine("Receiver Started!");
            Console.ReadLine();

            tcpProcessReceiver.Stop();
            receiver.Stop();


        }
    }
}
