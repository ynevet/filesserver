using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace FileServer.Server
{
    public class Server
    {
        private readonly IRequestHandler _handler;
        private readonly ILogger _logger;
        private readonly HttpListener _listener;

        public Server(IRequestHandler handler, ILogger logger)
        {
            Hostname = ConfigurationManager.AppSettings["hostname"];
            Port = int.Parse(ConfigurationManager.AppSettings["port"]);
          
            _handler = handler;
            _logger = logger;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://{Hostname}:{Port}/");
        }

        public void Start()
        {
            _listener.Start();
            Task.Factory.StartNew(ReceiveIncomingRequests, TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }

        public string Hostname { get; }
        public int Port { get; }

        private void ReceiveIncomingRequests()
        {
            while ( _listener.IsListening )
            {
                try
                {
                    var context = _listener.GetContext();
                    Task.Factory.StartNew(() => _handler.HandleRequest(context));
                }
                catch (Exception ex)
                {
                    _logger.LogError("ERROR OCCOURED WHEN PROCESSING CLIENT REQUEST", ex);
                }
            }
        }
    }
}