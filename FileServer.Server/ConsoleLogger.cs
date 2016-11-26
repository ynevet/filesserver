using System;

namespace FileServer.Server
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void LogError(string message, Exception ex)
        {
            Console.WriteLine($"ERROR: AT {DateTime.Now}, MESSAGE: {message}, EXCEPTION: {ex.Message}");
        }
    }
}
