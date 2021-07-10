using System;

namespace Ntk8.Exceptions
{
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException(string message) : base(message)
        {
            
        }
    }
}