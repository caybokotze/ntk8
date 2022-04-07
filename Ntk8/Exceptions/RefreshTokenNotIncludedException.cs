using System;

namespace Ntk8.Exceptions
{
    public class RefreshTokenNotIncludedException : Exception
    {
        public RefreshTokenNotIncludedException() : base("The refresh token should be included for authenticated calls.")
        {
            
        }

        public RefreshTokenNotIncludedException(string message) : base(message)
        {
            
        }
    }
}