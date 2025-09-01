using System;
using System.Net;
using NetworkBase;
using System.Threading;
using UDPServer_StateSync;
namespace UDPServer_InputSync
{
    
    internal class Program
    {
        public static void Main(string[] args)
        {
            PacketFactoryBase factory = new InputSynchronizationFactory();
            factory.Init();
            UDPServer_Demo_InputSync server = new UDPServer_Demo_InputSync();
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