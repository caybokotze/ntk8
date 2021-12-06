using System;

namespace Ntk8.Exceptions
{
    public class InvalidRefreshTokenException : Exception
    {
        public InvalidRefreshTokenException() : base("Refresh token has expired or is invalid.")
        {
            
        }
    }
}