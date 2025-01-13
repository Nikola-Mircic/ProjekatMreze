using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ProcessOS;

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
                if(socket != null && socket.Connected)
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

                    Process process = null;
                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        process = bf.Deserialize(ms) as Process;   
                    }

                    if(process == null)
                    {
                        Console.WriteLine("Invalid process was received");
                        break;
                    }

                    Console.WriteLine("Received process:\n\t" + process);
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
