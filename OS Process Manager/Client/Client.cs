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
            // Uticnica za klijnta
            // Svuda koristim Loopback posto rade na istoj masini
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint client = new IPEndPoint(IPAddress.Loopback, 9000);
            socket.Bind(client);

            // Adresa serverske UDP uticnice za prijavljivanje
            IPEndPoint server = new IPEndPoint(IPAddress.Loopback, 8000);

            byte[] msg = Encoding.UTF8.GetBytes("CONNECT");
            try
            {
                socket.SendTo(msg, server);
                Console.WriteLine("Message sent!");


                byte[] targetEndPoint = new byte[1024]; // Buffer za cuvanje adrese
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // Promenjiva za cuvanje adrese posiljaoca
                int bytesReceived = socket.ReceiveFrom(targetEndPoint, SocketFlags.None, ref remoteEndPoint);

                // Ako je posaljilac server -> ispisi adresu iz odgovora
                if (remoteEndPoint.Equals(server))
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
