namespace FileServer.Server
{
    public interface IAuthenticationProvider
    {
        AuthenticateResult AuthenticateUser(string userName, string password, string token);
        AuthenticateResult LogOff(string token);
    }
}