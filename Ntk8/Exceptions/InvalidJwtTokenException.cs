using System;

namespace Ntk8.Exceptions
{
    public class InvalidJwtTokenException : Exception
    {
        public InvalidJwtTokenException(string? message = null) : base(message ?? "The jwt token has expired or is invalid.")
        {
            
        }
    }
}