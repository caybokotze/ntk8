using System;

namespace Ntk8.Exceptions
{
    public class RefreshTokenNotIncludedException : Exception
    {
        public RefreshTokenNotIncludedException(string? message = null) : base(message ?? "The refresh token should be included for authenticated calls.")
        {
            
        }
    }
}