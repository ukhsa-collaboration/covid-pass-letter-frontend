using System;

namespace CovidLetter.Frontend.Pds
{
    public class PdsGenerateAccessTokenException : Exception
    {
        public PdsGenerateAccessTokenException()
        {
        }

        public PdsGenerateAccessTokenException(string message)
            : base(message)
        {
        }

        public PdsGenerateAccessTokenException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
