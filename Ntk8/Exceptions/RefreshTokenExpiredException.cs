using System;

namespace Ntk8.Exceptions
{
    public class RefreshTokenExpiredException : Exception
    {
        public RefreshTokenExpiredException() : base("Refresh token has expired.")
        {
            
        }
    }
}