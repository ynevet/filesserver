using System;

namespace FileServer.Server
{
    public class Runner
    {
        public static void Main(string[] args)
        {
            var consoleLogger = new ConsoleLogger();
            var serverHandler = new RequestHandler(new FilesManager(), new ConsoleLogger(), new ServerAuthenticationProvider());
            var server = new FileServer.Server.Server(serverHandler, consoleLogger);

            server.Start();
            Console.Title = "File Server v1.0";
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
