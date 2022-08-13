using System;

namespace Ntk8.Exceptions
{
    public class InvalidTokenLengthException : Exception
    {
        public InvalidTokenLengthException(string? message = null) : base(message ?? "Invalid token length. The token secret must be at least 32 characters long to meet AES 128 standards.")
        {
            
        }
    }
}