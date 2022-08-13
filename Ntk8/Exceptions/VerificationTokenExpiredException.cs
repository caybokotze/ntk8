using System;

namespace Ntk8.Exceptions
{
    public class VerificationTokenExpiredException : Exception
    {
        public VerificationTokenExpiredException(string? message = null) : base(message ?? "The verification token has expired. Please register again to regenerate the token.")
        {
            
        }
    }
}