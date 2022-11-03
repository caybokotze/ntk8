using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions
{
    [Serializable]
    public class VerificationTokenExpiredException : Exception
    {
        protected VerificationTokenExpiredException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
        {
            
        }
        
        public VerificationTokenExpiredException(string? message = null) : base(message ?? "The verification token has expired. Please register again to regenerate the token.")
        {
            
        }
    }
}