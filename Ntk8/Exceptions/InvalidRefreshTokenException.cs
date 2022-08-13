using System;

namespace Ntk8.Exceptions
{
    public class InvalidRefreshTokenException : Exception
    {
        public InvalidRefreshTokenException(string? message = null) : base(message ?? "Refresh token has expired or is invalid.")
        {
            
        }
    }
}