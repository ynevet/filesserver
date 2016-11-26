using System;

namespace FileServer.Server
{
    public class CustomerFacingException : Exception
    {
        public CustomerFacingException()
        {
        }

        public CustomerFacingException(string message)
            : base(message)
        {
        }

        public CustomerFacingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}