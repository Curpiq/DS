using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Chain
{
    class Program
    {
        private static Socket _sender;
        private static Socket _listener;

        static void Main(string[] args)
        {
            try
            {
                Arguments arguments = ParseArgs(args);

                CreateConnection(arguments.listeningPort, arguments.address, arguments.port);

                if (arguments.isInitiator) 
                {
                    Initiator();
                }
                else
                {
                    NormalProcess();
                }

                _sender.Shutdown(SocketShutdown.Both);
                _sender.Close();
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }

            System.Console.ReadLine();
        }

        private static void CreateConnection(int listeningPort, string address, int port)
        {
            _listener = GetListnerSettings(listeningPort);
            _sender = GetSenderSettings(address, port);
        }

        private static Socket GetListnerSettings(int listeningPort)
        {
            IPAddress listenIpAddress = IPAddress.Any;
            IPEndPoint localEP = new IPEndPoint(listenIpAddress, listeningPort);

            Socket listener = new Socket(
                 listenIpAddress.AddressFamily,
                 SocketType.Stream,
                 ProtocolType.Tcp);

            listener.Bind(localEP);
            listener.Listen(10);

            return listener;
        }

        private static Socket GetSenderSettings(string address, int port)
        {
            IPAddress ipAddress = (address == "localhost") ? IPAddress.Loopback : IPAddress.Parse(address);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            Socket sender = new Socket(
            ipAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

            try
            {
                sender.Connect(remoteEP);
            }
            catch (SocketException)
            {
                Console.WriteLine("Failed to connecting");
            }

            return sender;
        }

        private static void Initiator()
        {
            int x = Convert.ToInt32(Console.ReadLine());

            _sender.Send(BitConverter.GetBytes(x));
        
            Socket handler = _listener.Accept();
            byte[] buf = new byte[sizeof(int)];           
            handler.Receive(buf);   

            int y = BitConverter.ToInt32(buf);

            _sender.Send(BitConverter.GetBytes(y));

            Console.WriteLine(y);

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

        private static void NormalProcess()
        {
            int x = Convert.ToInt32(Console.ReadLine());

            Socket handler = _listener.Accept(); 
            byte[] buf = new byte[sizeof(int)];
            handler.Receive(buf);

            int y = BitConverter.ToInt32(buf);

            _sender.Send(BitConverter.GetBytes(Math.Max(x, y)));
        
            handler.Receive(buf);

            _sender.Send(buf);

            Console.WriteLine(BitConverter.ToInt32(buf));

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
        private static Arguments ParseArgs(string[] args)
        {
            if (args.Length < 2 || args.Length > 4)
            {                
                throw new ArithmeticException("Invalid arguments count");
            }

            Arguments arguments = new Arguments();
            arguments.listeningPort = Int32.Parse(args[0]);
            arguments.address = args[1];
            arguments.port = Int32.Parse(args[2]);

            if (args.Length == 4 && args[3] == "true")
            {
                arguments.isInitiator = true;
            }

            return arguments;
        }

    }
}
