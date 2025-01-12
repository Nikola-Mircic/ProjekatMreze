using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Client
    {
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint client = new IPEndPoint(IPAddress.Loopback, 9000);
            socket.Bind(client);

            IPEndPoint server = new IPEndPoint(IPAddress.Loopback, 8000);

            byte[] msg = Encoding.UTF8.GetBytes("CONNECT");
            try
            {
                socket.SendTo(msg, server);
                Console.WriteLine("Message sent!");

                byte[] targetEndPoint = new byte[1024];
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int bytesReceived = socket.ReceiveFrom(targetEndPoint, SocketFlags.None, ref remoteEndPoint);

                if (bytesReceived == 0)
                    return;

                if (remoteEndPoint.Equals(remoteEndPoint))
                {
                    Console.WriteLine(Encoding.UTF8.GetString(targetEndPoint));
                    Console.ReadLine();
                }

            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
