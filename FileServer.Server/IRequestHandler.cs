namespace FileServer.Server
{
    public interface IRequestHandler
    {
        void HandleRequest(object state);
    }
}