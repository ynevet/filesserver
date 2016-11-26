using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HttpMultipartParser;

namespace FileServer.Server
{
    public class RequestHandler : IRequestHandler
    {
        private readonly IFilesManager _filesManager;
        private readonly ILogger _logger;
        private readonly IAuthenticationProvider _authProvider;
        private MultipartFormDataParser _requestFormData;
        private HttpListenerContext _context;

        private readonly List<string> _validRequestsMethods = new List<string>
        {
            Constants.Methods.List,
            Constants.Methods.Get,
            Constants.Methods.Put,
            Constants.Methods.Logoff
            
        };

        public RequestHandler(IFilesManager filesManager, ILogger logger, IAuthenticationProvider authProvider)
        {
            _filesManager = filesManager;
            _logger = logger;
            _authProvider = authProvider;
        }

        public void HandleRequest(object state)
        {
            var requestId = "NO-ID-GENERATED";
            try
            {
                _context = (HttpListenerContext) state;
                _requestFormData = new MultipartFormDataParser(_context.Request.InputStream);

                requestId = Guid.NewGuid().ToString("N");

                _logger.Log($"INCOMING REQUEST: {DateTime.Now} - ID:{requestId}, FROM:{_context.Request.RemoteEndPoint}, URL:{_context.Request.Url}");

                if (AuthenticateRequest()) //AUTHENTICATE REQUEST
                {
                    RouteRequestForProcessing(_context);
                }
            }
            catch (CustomerFacingException ex)
            {
                _logger.LogError($"{DateTime.Now} - ERROR OCCOURED WHEN PROCESSING REQUEST: {requestId}", ex);

                _context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                _context.Response.StatusDescription = "REQUEST IS INVALID. SEE RESPONSE MESSAGE.";
                _context.Response.ContentType = "text/plain";

                var msgBytes = Encoding.UTF8.GetBytes(ex.Message);
                _context.Response.OutputStream.Write(msgBytes, 0, msgBytes.Length);

            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"{DateTime.Now} - ERROR OCCOURED WHEN HANDLING REQUEST: {requestId}, REQUEST URL: {_context.Request.Url}",
                    ex);
                _context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                _context.Response.StatusDescription = "SERVER ERROR OCCURRED";
                _context.Response.ContentType = "text/plain";
            }
            finally
            {
                _context.Response.OutputStream.Close();
                _context.Response.Close();
            }
        }

        private void RouteRequestForProcessing(HttpListenerContext context)
        {
            //REQUEST VALIDATION
            string method;
            var requestSegments = ValidateRequest(context, out method);

            //REQUEST ROUTING AND PROCESSING
            if (method.Equals(Constants.Methods.List, StringComparison.OrdinalIgnoreCase)) // HANDLE "LIST"
            {
                _filesManager.ListAllFiles(context);
            }
            else if (method.Equals(Constants.Methods.Get, StringComparison.OrdinalIgnoreCase)) // HANDLE "GET"
            {
                if (requestSegments.Length < 3)
                {
                    throw new CustomerFacingException($"THE {method} METHOD SHOULD BE PROVIDED WITH PARAMETERS");
                }

                var methodParams = requestSegments[2];
                var fileToSend = methodParams.Split(' ').First();
                _filesManager.SendFile(context, $@"Files\{fileToSend}");
            }
            else if (method.Equals(Constants.Methods.Put, StringComparison.OrdinalIgnoreCase)) // HANDLE "PUT"
            {
                try
                {
                    foreach (var file in _requestFormData.Files)
                    {
                        _filesManager.StoreFile(file.FileName, file.Data);
                    }
                    
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.ContentType = "text/html";
                    using (var writer = new StreamWriter(context.Response.OutputStream, Encoding.UTF8))
                    {
                        writer.WriteLine("UPLOADED SUCCEED");
                    }
                }
                catch (Exception)
                {
                    throw new CustomerFacingException($"FAILED TO UPLOAD FILE - CHECK THE FILE OR TRY AGAIN LATER..");
                }
            }
            else if(method.Equals(Constants.Methods.Logoff, StringComparison.OrdinalIgnoreCase))
            {

            }
        }

        private  string[] ValidateRequest(HttpListenerContext context, out string givenMethod)
        {
            var requestSegments = context.Request.Url.Segments;
            if (requestSegments.Length < 2)
            {
                throw new CustomerFacingException("NO METHOD PROVIDED FOR REQUEST. POSSIBLE METHODS: 'LIST', 'GET' AND 'PUT'");
            }

            givenMethod = requestSegments[1].Trim('/').ToLower();
            if ( !_validRequestsMethods.Contains(givenMethod) )
            {
                throw new CustomerFacingException($"INVALID METHOD WAS PROVIDED FOR REQUEST: {givenMethod}");
            }
            return requestSegments;
        }

        private bool AuthenticateRequest()
        {
            string password;
            string token;
            var user = GetUserCredentials(out password, out token);

            var authResult = _authProvider.AuthenticateUser(user, password, token);

            if (authResult.IsAuthenticated == false)
            {
                _context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                _context.Response.StatusDescription = "THE REQUEST IS UNAUTHENTICATED, PLEASE LOGIN WITH VALID CREDENTILS";
                _context.Response.ContentType = "text/plain";

                return false;
            }

            if (authResult.IsAuthenticated && authResult.NewTokenCreated)
            {
                _context.Response.StatusCode = (int) HttpStatusCode.OK;
                _context.Response.StatusDescription = "LOGIN SUCCEED, TOKEN IS A ATTACHED";
                _context.Response.ContentType = "text/plain";

                var tokenBytes = Encoding.UTF8.GetBytes(authResult.Token);
                _context.Response.OutputStream.Write(tokenBytes, 0, tokenBytes.Length);

                return false;
            }

            return true;
        }

        private string GetUserCredentials(out string password, out string token)
        {
            string user = null;
            password = null;
            token = null;

            if (_requestFormData.HasParameter("username"))
            {
                user = _requestFormData.GetParameterValue("username");
            }

            if (_requestFormData.HasParameter("password"))
            {
                password = _requestFormData.GetParameterValue("password");
            }

            if (_requestFormData.HasParameter("token"))
            {
                token = _requestFormData.GetParameterValue("token");
            }
            return user;
        }
    }
}