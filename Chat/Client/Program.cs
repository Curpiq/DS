using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Text.Json;

namespace Client
{
    class Program
    {
        public static void StartClient(string address, int port, string message)
        {
            try
            {
                IPAddress ipAddress = address == "localhost" ? IPAddress.Loopback : IPAddress.Parse(address);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // CREATE
                Socket sender = new Socket(
                    ipAddress.AddressFamily,
                    SocketType.Stream, 
                    ProtocolType.Tcp);

                try
                {
                    // CONNECT
                    sender.Connect(remoteEP);

                    // Подготовка данных к отправке
                    byte[] msg = Encoding.UTF8.GetBytes(message + "<EOF>");

                    // SEND
                    int bytesSent = sender.Send(msg);

                    // RECEIVE
                    byte[] buf = new byte[1024];
                    StringBuilder historyStr = new StringBuilder();
                    int bytesRec = 0;

                    do
                    {
                        bytesRec = sender.Receive(buf);
                        historyStr.Append(Encoding.UTF8.GetString(buf, 0, bytesRec));;
                    }
                    while (bytesRec > 0);

                    List<string> history = JsonSerializer.Deserialize<List<string>>(historyStr.ToString());
                    
                    foreach (var msgStr in history)
                    {
                        Console.WriteLine(msgStr);
                    }

                    // RELEASE
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                throw new ArgumentException("Invalid number of arguments");
            }
            string address = args[0];
            int port = Int32.Parse(args[1]);
            string message = args[2];

            StartClient(address, port, message);
        }
    }
}
