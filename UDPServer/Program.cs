using System;
using System.Net;
using NetworkBase;
using System.Threading;
namespace UDPServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            PacketFactoryBase factory = new StateSynchronizationFactory();
            factory.Init();
            UDPServer_Demo server = new UDPServer_Demo();
            if (!server.Init(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8880),0,2,4,50))
            {
                Logger.LogToTerminal("Failed to Init UDP server");
                return;
            }
            server.Start();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; 
                server.Stop();
                Environment.Exit(0);
            };
            Thread.Sleep(Timeout.Infinite);
        }
    }
}