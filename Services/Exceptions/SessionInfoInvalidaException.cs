using System;


namespace InCorpApp.Services.Exceptions
{
    public class SessionInfoInvalidaException : ApplicationException
    {
        public SessionInfoInvalidaException(string message) : base(message)
        {
        }
    }
}


