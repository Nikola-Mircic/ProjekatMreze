﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client.Senders
{
    internal class UdpRequestSender
    {
        private Socket clientSocket = null;
        private IPEndPoint serverEndPoint = null;

        public int ConnectionAttempts { get; private set; }

        public UdpRequestSender(IPAddress IPAddress, int serverPort)
        {
            serverEndPoint = new IPEndPoint(IPAddress, serverPort);
            ConnectionAttempts = 0;
        }

        public void Close()
        {
            if(clientSocket != null)
            {
                clientSocket.Close();
            }
        }

        public (string ip, int port, bool success) RequestConnection()
        {
            if(clientSocket == null)
            {
                try
                {
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return ("", -1, false);
                }
            }

            byte[] msg = Encoding.UTF8.GetBytes("CONNECT");
            try
            {
                clientSocket.SendTo(msg, serverEndPoint);
                Console.WriteLine("Request sent!");

                byte[] targetEndPoint = new byte[1024]; // Buffer za cuvanje adrese
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // Promenjiva za cuvanje adrese posiljaoca
                int bytesReceived = clientSocket.ReceiveFrom(targetEndPoint, ref remoteEndPoint);

                // Ako je posaljilac server -> ispisi adresu iz odgovora
                if (remoteEndPoint.Equals(serverEndPoint))
                {
                    string[] reply = Encoding.UTF8.GetString(targetEndPoint).Split(':');
                    Console.WriteLine(Encoding.UTF8.GetString(targetEndPoint));
                    return (reply[0], int.Parse(reply[1]), true);
                }

                ConnectionAttempts++;
                return ("", -1, false);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ConnectionAttempts++;
                return ("", -1, false);
            }
        }

    }
}
