using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Server.Receivers
{
    internal class ProcessReceiver
    {
        private Socket tcpServerSocket = null;

        private IPAddress ipAddress;
        private int port;

        private List<Socket> acceptedSockets = new List<Socket>(); //tmp resenje za gasenje tcp konekcija nakon sto se ugasi server

        public ProcessReceiver(IPAddress ip, int port)
        {
            this.ipAddress = ip;
            this.port = port;
        }

        public bool Start()
        {
            if(tcpServerSocket == null)
            {
                try
                {
                    tcpServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
                    tcpServerSocket.Bind(ipEndPoint);
                    tcpServerSocket.Listen(10);
                    return true;
                }
                catch(SocketException ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            return true;
        }

        public void Stop()
        {
            if (tcpServerSocket != null)
            {
                tcpServerSocket.Close();
            }
            foreach (Socket socket in acceptedSockets) //zatvaranje otvorenih soketa ukoliko se listener ugasi pre njih
            {
                if(socket != null || socket.IsBound)
                {
                    socket.Close();
                }
            }
        }

        public void Connect()
        {
            tcpServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), tcpServerSocket);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                if (tcpServerSocket == null || !tcpServerSocket.IsBound)
                {
                    return; 
                }

                Socket acceptedSocket = tcpServerSocket.EndAccept(ar);
                acceptedSockets.Add(acceptedSocket);
                Thread tcpListener = new Thread(() => HandleClient(acceptedSocket));
                tcpServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), tcpServerSocket);
                tcpListener.Start();
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("TCP listener socket closed!");
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void HandleClient(Socket socket)
        {
            byte[] buffer = new byte[1024];
            string msg = "";
            try
            {
                while (true)
                {
                    int bytesReceived = socket.Receive(buffer);

                    if (bytesReceived == 0)
                    {
                        Console.WriteLine("Client disconnected");
                        break;
                    }

                    msg = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                    Console.WriteLine("Received message: " + msg);

                    if (msg == "QUIT")
                    {
                        Console.WriteLine("Received QUIT. Closing connection...");
                        break;
                    }
                }
            }
            catch (SocketException ex)
            {
                if(ex.SocketErrorCode == SocketError.ConnectionAborted)
                {
                    Console.WriteLine("TCP socket closed!");
                }
                else
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                socket.Close();
                Console.WriteLine("Connection closed.");
            }
        }

    }
}
