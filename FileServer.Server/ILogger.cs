using System;

namespace FileServer.Server
{
    public interface ILogger
    {
        void Log(string message);

        void LogError(string message, Exception ex);
    }
}