﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server.Receivers
{
    /*
        Klasa koja prima zahtev za prijavu od klijenta i povezuje ih sa odgovarajucim TCP socket-om
     */
    public class ClientReceiver
    {
        private readonly int Port; // Port na kome uticnica radi
        private Socket ClientSocket; // UDP Socket za prijem zahteva
        private Thread ReceiverThread; // Thread na kome radi socket

        private bool Running = false;

        public IPEndPoint ProcessReceiver { get; set; }

        public ClientReceiver(int port)
        {
            // Inicijalizacija promenjivih 
            this.Port = port;

            this.ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.ClientSocket.Bind(new IPEndPoint(IPAddress.Any, Port));

            this.ReceiverThread = new Thread(() =>
            {
                HandleRequest();
            });
        }

        private void HandleRequest()
        {
            byte[] msg = new byte[1024];
            EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (Running)
            {
                try
                {
                    int bytesReceived = ClientSocket.ReceiveFrom(msg, SocketFlags.None, ref clientEndPoint);
                    
                    string receivedMessage = Encoding.UTF8.GetString(msg, 0, bytesReceived);

                    if (receivedMessage.Equals("CONNECT"))
                    {
                        byte[] process_ep = Encoding.UTF8.GetBytes(ProcessReceiver.ToString());
                        ClientSocket.SendTo(process_ep, clientEndPoint);
                    }
                    else if(receivedMessage.Equals("OS-STATUS"))
                    {

                    }
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.Interrupted)
                        return;

                    Console.WriteLine("[ClientReceiver] Socket error {0}: {1}", ex.SocketErrorCode, ex.Message);
                }
            }
        }

        public void Start()
        {
            if(Running) return;

            Running = true;
            ReceiverThread.Start();
        }

        public void Stop()
        {
            if (!Running) return;

            Running = false;
            ClientSocket.Close();
            ReceiverThread.Join();
        }
    }
}
