using System.IO;
using System.Net;

namespace FileServer.Server
{
    public interface IFilesManager
    {
        void StoreFile(string fileName, Stream fileData);
        void ListAllFiles(HttpListenerContext context);
        void SendFile(HttpListenerContext ctx, string filePath);
    }
}