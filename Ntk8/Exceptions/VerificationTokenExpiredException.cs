using System;

namespace Ntk8.Exceptions
{
    public class VerificationTokenExpiredException : Exception
    {
        public VerificationTokenExpiredException() : base("The verification token has expired.")
        {
            
        }
    }
}