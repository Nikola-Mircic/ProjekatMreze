using Server.Receivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            receiver.Start();
            Console.WriteLine("Receiver Started!");
            Console.ReadLine();

            receiver.Stop();
        }
    }
}
