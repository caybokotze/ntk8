using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions
{
    [Serializable]
    public class RefreshTokenNotIncludedException : Exception
    {
        protected RefreshTokenNotIncludedException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
        {
            
        }
        
        public RefreshTokenNotIncludedException(string? message = null) : base(message ?? "The refresh token should be included for authenticated calls.")
        {
            
        }
    }
}