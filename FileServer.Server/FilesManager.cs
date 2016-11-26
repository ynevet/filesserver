using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;

namespace FileServer.Server
{
    public class FilesManager : IFilesManager
    {
        public void StoreFile(string fileName, Stream stream)
        {
            using (var fileStream = File.Create(fileName))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
        }

        public void ListAllFiles(HttpListenerContext context)
        {
            var files = string.Join("\n", Directory.GetFiles("Files").Select(Path.GetFileName).ToArray());
            var bytes = Encoding.UTF8.GetBytes(files);
            using (var stream = context.Response.OutputStream)
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void SendFile(HttpListenerContext ctx, string filePath)
        {
            var response = ctx.Response;
            try
            {
                using (var fs = File.OpenRead(filePath))
                {
                    var filename = Path.GetFileName(filePath);
                    response.ContentLength64 = fs.Length;
                    response.SendChunked = false;
                    response.ContentType = MediaTypeNames.Application.Octet;
                    response.AddHeader("Content-disposition", "attachment; filename=" + filename);

                    var buffer = new byte[64 * 1024];
                    using (var bw = new BinaryWriter(response.OutputStream))
                    {
                        int read;
                        while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.Write(buffer, 0, read);
                            bw.Flush();
                        }
                    }

                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.StatusDescription = "OK";
                }
            }
            catch (Exception)
            {
                throw new CustomerFacingException($"The requested file '{Path.GetFileName(filePath)}' was not found");
            }
        }
    }
}
