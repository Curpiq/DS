using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Text.Json;


namespace Server
{
    class Program
    {
        private static List<string> _history = new List<string>(); 
        public static void StartListening(string port)
        {
            // Привязываем сокет ко всем интерфейсам на текущей машинe
            IPAddress ipAddress = IPAddress.Any; 
            
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Convert.ToInt32(port));

            // CREATE
            Socket listener = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            try
            {
                // BIND
                listener.Bind(localEndPoint);

                // LISTEN
                listener.Listen(10);

                while (true)
                {
                    // ACCEPT
                    Socket handler = listener.Accept();
                    
                    byte[] prefixBuf = new byte[4];
                    int prefix = handler.Receive(prefixBuf);
                    int size = BitConverter.ToInt32(prefixBuf, 0);
                    
                    //Console.WriteLine(size);

                    byte[] buf = new byte[size];
                    string data = null;

                    while (size > 0)
                    {
                        // RECEIVE
                        int bytesRec = handler.Receive(buf);

                        data += Encoding.UTF8.GetString(buf, 0, bytesRec);

                        size -= bytesRec;
                        Console.WriteLine(size);
                    }

                    _history.Add(data);
                    Console.WriteLine("Message received: {0}", data);

                    // Отправляем текст обратно клиенту
                    byte[] msg = new byte[size];
                    msg = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_history));

                    // SEND
                    handler.Send(msg);

                    // RELEASE
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        static void Main(string[] args)
        {
             if (args.Length != 1)
            {
                throw new ArgumentException("Invalid arguments count");
            }

            StartListening(args[0]);
        }
    }
}