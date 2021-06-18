using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;


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

                    byte[] buf = new byte[1024];
                    string data = null;
                    int messageLength;
                    while (true)
                    {
                        // RECEIVE
                        int bytesRec = handler.Receive(buf);

                        data += Encoding.UTF8.GetString(buf, 0, bytesRec);

                        string prefix = "";
                        for (int i = data.Length - 2; i >= 0; i--)
                        {
                            char ch = data[i];
                            if (ch == '<')
                            {
                                break;
                            }
                            prefix += data[i];
                        }
                        string prefixValue = new string(prefix.ToCharArray().Reverse().ToArray());
                        messageLength = Convert.ToInt32(prefixValue);

                        if (data.IndexOf("<" + prefixValue + ">") > -1)
                        {
                            break;
                        }
                    }

                    data = data.Remove(messageLength);
                    _history.Add(data);
                    Console.WriteLine("Message received: {0}", data);

                    // Отправляем текст обратно клиенту
                    byte[] msg = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_history));

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