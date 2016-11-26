using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using FileServer.DataAccess;
using FileServer.DataAccess.Models;
using HttpMultipartParser;
using Jose;
using Newtonsoft.Json.Linq;
using static System.Double;

namespace FileServer.Server
{
    public class ServerAuthenticationProvider : IAuthenticationProvider
    {
        private const string SecretKey = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";
        private readonly string _domain = ConfigurationManager.AppSettings.Get("hostname");
        private readonly byte[] _secretKey = Base64UrlDecode(SecretKey);

        public AuthenticateResult AuthenticateUser(string userName, string password, string givenToken)
        {
            if (IsRequestProvidedWithValidToken(givenToken))
            {
                return new AuthenticateResult {IsAuthenticated = true};
            }

            var user = TryGetConfirmedUserCredentials(userName, password);
            if ( user == null )
            {
                return new AuthenticateResult {IsAuthenticated = false};
            }

            var token = GenerateJwtToken(user.UserName, _domain);

            return new AuthenticateResult {IsAuthenticated = true, Token = token};
        }

        public AuthenticateResult LogOff(string token)
        {
            throw new NotImplementedException();
        }

        public void RegisterUserSecurely(string userName, string password)
        {
            using (var userContext = new UserContext())
            {
                var hash = new PasswordHashingProvider(password);
                var hashBytes = hash.ToArray();
                var savedPasswordHash = Convert.ToBase64String(hashBytes);

                userContext.Users.Add(new User
                {
                    UserName = userName,
                    Password = savedPasswordHash,
                    AddedAt = DateTime.UtcNow
                });
                userContext.SaveChanges();
            }
        }

        private bool IsRequestProvidedWithValidToken(string givenToken)
        {
            if (givenToken == null)
            {
                return false;
            }

            var decodedToken = JWT.Decode(givenToken, _secretKey, JwsAlgorithm.HS256);
            var jwtObject = JObject.Parse(decodedToken);
            var tokenExpirationDate = jwtObject["exp"].ToString();

            double parseResult;
            var convertionResult = TryParse(tokenExpirationDate, out parseResult);
            if (convertionResult)
            {
                var expirationDate = UnixTimeStampToDateTime(parseResult);
                if (expirationDate > DateTime.UtcNow)
                {
                    return true;
                }
            }

            return false;
        }

        private static User TryGetConfirmedUserCredentials(string username, string password)
        {
            using (var userContext = new UserContext())
            {
                var user = userContext.Users.SingleOrDefault(x => x.UserName == username);
                if (user != null)
                {
                    var savedPasswordHash = user.Password;
                    var hashBytes = Convert.FromBase64String(savedPasswordHash);
                    var hashingProvider = new PasswordHashingProvider(hashBytes);

                    if (hashingProvider.Verify(password))
                    {
                        return user;
                    }
                }
                return null;
            }
        }

        private static MultipartFormDataParser ExtractRequestFormData(HttpListenerContext context)
        {
            try
            {
                var requestFormData = new MultipartFormDataParser(context.Request.InputStream);

                return requestFormData;
            }
            catch (Exception)
            {
                throw new CustomerFacingException("BAD REQUEST: NO FORM DATA SUPPLIED");
            }
        }

        private string GenerateJwtToken(string clientId, string domain)
        {
            var issued = DateTime.UtcNow;
            var expire = DateTime.UtcNow.AddHours(12);

            var payload = new Dictionary<string, object>()
            {
                {"iss", $"https://{domain}/"},
                {"aud", clientId},
                {"sub", "anonymous"},
                {"iat", ToUnixTimeStamp(issued).ToString()},
                {"exp", ToUnixTimeStamp(expire).ToString()}
            };
            
            var token = JWT.Encode(payload, _secretKey, JwsAlgorithm.HS256);

            return token;
        }

        private static byte[] Base64UrlDecode(string arg)
        {
            var s = arg;
            s = s.Replace('-', '+');
            s = s.Replace('_', '/');

            switch (s.Length % 4)
            {
                case 0: break;
                case 2: s += "=="; break;
                case 3: s += "="; break;
                default:
                    throw new Exception(
             "Illegal base64url string!");
            }

            return Convert.FromBase64String(s);
        }

        private static long ToUnixTimeStamp(DateTime dateTime)
        {
            return (int)(dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();

            return dtDateTime;
        }
    }
}