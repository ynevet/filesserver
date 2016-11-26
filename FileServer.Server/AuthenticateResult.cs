namespace FileServer.Server
{
    public class AuthenticateResult
    {
        public bool IsAuthenticated { get; set; }

        public string Token { get; set; }

        public bool NewTokenCreated => Token != null;
    }
}