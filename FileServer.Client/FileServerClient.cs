using System;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace FileServer.Client
{
    public class FileServerClient
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private string _loginToken;

        public FileServerClient(string serverIp, int serverPort)
        {
            string serverHostname = $"http://{serverIp}:{serverPort}";
            _httpClient.BaseAddress = new Uri(serverHostname);
        }

        public bool Connect(string user, string password)
        {
            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(user), "username");
                content.Add(new StringContent(password), "password");

                var result = _httpClient.PostAsync("/", content).Result;

                if (!result.IsSuccessStatusCode)
                {
                    return result.IsSuccessStatusCode;
                }

                var resultContent = result.Content.ReadAsStringAsync().Result;

                _loginToken = resultContent;

                return result.IsSuccessStatusCode;
            }
        }

        public string List()
        {
            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(_loginToken), "token");

                var result = _httpClient.PostAsync("/List", content).Result;
                var resultContent = result.Content.ReadAsStringAsync().Result;

                return resultContent;
            }
        }

        public string GetFiles(string[] fileNames)
        {
            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(_loginToken), "token");

                var result = _httpClient.PostAsync($"/get/{fileNames.FirstOrDefault()}", content).Result;
                var resultContent = result.Content.ReadAsByteArrayAsync().Result;

                if (resultContent != null)
                {
                    var first = fileNames.FirstOrDefault(); //CAN'T DOWNLOAD MULTIPLE FILES IN A SINGLE REQUEST USING THE HTTP PROTOCOL
                    if (first != null)
                    {
                        File.WriteAllBytes(first, resultContent);
                    }
                    
                    return "DOWNLOAD COMPLETED";
                }

                return "DOWNLOAD FAILED";
            }
        }

        public string SendFiles(string[] fileNames)
        {
            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(_loginToken), "token");

                foreach (var file in fileNames)
                {
                    var fileBytes = File.ReadAllBytes(file);
                    content.Add(new ByteArrayContent(fileBytes, 0, fileBytes.Length), $"file-{file}", file);
                }

                var result = _httpClient.PostAsync("/put", content).Result;
                var resultContent = result.Content.ReadAsStringAsync().Result;

                return resultContent;
            }
        }

        public void Disconnect()
        {
            _loginToken = null;
        }
    }
}