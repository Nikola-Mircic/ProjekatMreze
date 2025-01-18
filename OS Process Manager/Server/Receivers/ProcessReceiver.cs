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
using Server.Managers;

namespace Server.Receivers
{
    internal class ProcessReceiver
    {
        private Socket tcpServerSocket = null;
        private IPEndPoint endPoint = null;
        private int MaxClients;
        private List<Socket> clients = new List<Socket>();
        bool running;

        public ProcessReceiver(IPAddress ip, int port, int maxClients)
        {
            endPoint = new IPEndPoint(ip, port);
            MaxClients = maxClients;

        }

        public void Start()
        {
            tcpServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpServerSocket.Bind(endPoint);
            tcpServerSocket.Blocking = false;
            tcpServerSocket.Listen(MaxClients);
            running = true;

            byte[] buffer = new byte[1024];
            try
            {
                while (running)
                {
                    List<Socket> checkRead = new List<Socket>();
                    List<Socket> checkError = new List<Socket>();

                    if (clients.Count < MaxClients)
                    {
                        checkRead.Add(tcpServerSocket);
                    }
                    checkError.Add(tcpServerSocket);

                    foreach (Socket socket in clients)
                    {
                        checkRead.Add(socket);
                        checkError.Add(socket);
                    }

                    Socket.Select(checkRead, null, checkError, 1000);

                    if (checkRead.Count > 0)
                    {
                        foreach (Socket socket in checkRead)
                        {
                            if (socket == tcpServerSocket)
                            {
                                Socket client = tcpServerSocket.Accept();
                                client.Blocking = false;
                                clients.Add(client);
                            }
                            else
                            {
                                int bytesReceived = socket.Receive(buffer);

                                if (bytesReceived == 0)
                                {
                                    Console.WriteLine("[TCP socket] client disconnected");
                                    socket.Close();
                                    clients.Remove(socket);
                                    continue;
                                }

                                Process process = Process.Deserialize(buffer);

                                if (process == null)
                                {
                                    Console.WriteLine("[TCP socket] invalid process received");
                                    continue;
                                }

                                //Console.WriteLine("Received process:\n\t" + process);
                                if (Manager.Get().Add(process))
                                {
                                    //posalji poruku klijentu da je proces prihvacen
                                    socket.Send(Encoding.UTF8.GetBytes("SUCCESS"));
                                }
                                else
                                {
                                    //posalji poruku klijentu da nema memorije/cpu
                                    socket.Send(Encoding.UTF8.GetBytes("FAILURE"));
                                }
                            }
                        }
                    }
                    checkRead.Clear();
                }
                foreach (Socket client in clients)
                {
                    client.Close();
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionAborted)
                {
                    Console.WriteLine("[TCP socket] closed!");
                }
                else
                {
                    Console.WriteLine($"[TCP socket] error:\n{ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error]:\n{ex.Message}");
            }
        }

        public void Stop(Thread t)
        {
            running = false;
            t.Join();
            tcpServerSocket.Close();
            Console.WriteLine("[TCP listener] socket closed!");
        }

    }
}
