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

                    checkRead.Add(tcpServerSocket);

                    foreach (Socket socket in clients)
                    {
                        checkRead.Add(socket);
                    }

                    Socket.Select(checkRead, null, null, 1000);

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
                                try
                                {
                                    if (socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0)
                                    {
                                        Console.WriteLine("[TCP socket] Client has closed the connection.");
                                        socket.Shutdown(SocketShutdown.Both);
                                        socket.Close();
                                        clients.Remove(socket);
                                        continue;
                                    }

                                    int bytesReceived = socket.Receive(buffer);

                                    if (bytesReceived == 0)
                                    {
                                        Console.WriteLine("[TCP socket] Client disconnected (0 bytes received).");
                                        socket.Shutdown(SocketShutdown.Both);
                                        socket.Close();
                                        clients.Remove(socket);
                                        continue;
                                    }

                                    Process process = Process.Deserialize(buffer);

                                    if (process == null)
                                    {
                                        Console.WriteLine("[TCP socket] Invalid process received.");
                                        continue;
                                    }

                                    if (Manager.Get().Add(process))
                                    {
                                        socket.Send(Encoding.UTF8.GetBytes("SUCCESS"));
                                    }
                                    else
                                    {
                                        socket.Send(Encoding.UTF8.GetBytes("FAILURE"));
                                    }
                                }
                                catch (SocketException ex)
                                {
                                    if (ex.SocketErrorCode == SocketError.ConnectionReset || ex.SocketErrorCode == SocketError.TimedOut)
                                    {
                                        Console.WriteLine("[TCP socket] Client forcibly closed the connection or timed out.");
                                        socket.Shutdown(SocketShutdown.Both);
                                        socket.Close();
                                        clients.Remove(socket);
                                    }
                                    else
                                    {
                                        Console.WriteLine($"[TCP socket] SocketException: {ex.Message}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[Error]: {ex.Message}");
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
