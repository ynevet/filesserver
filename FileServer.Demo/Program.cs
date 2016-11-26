using System;
using FileServer.Server;

namespace FileServer.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var consoleLogger = new ConsoleLogger();
            var serverHandler = new RequestHandler(new FilesManager(), new ConsoleLogger(), new ServerAuthenticationProvider());
            var server = new Server.Server(serverHandler, consoleLogger);

            server.Start();
            Console.WriteLine("FILES SERVER v1.0 IS RUNNING..");
            Console.WriteLine($"SERVER LISTENING ON HOSTNAME-{ server.Hostname}, PORT-{server.Port}");

            Console.WriteLine("PRESS ANY KEY TO TERMINATE");
            Console.WriteLine();

            Console.ReadKey();
            Console.WriteLine("STOPPING THE SERVER..");
            server.Stop();
        }
    }
}
