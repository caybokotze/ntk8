using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions
{
    [Serializable]
    public class InvalidRefreshTokenException : Exception
    {
        protected InvalidRefreshTokenException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
        {
            
        }
        
        public InvalidRefreshTokenException(string? message = null) : base(message ?? "Refresh token has expired or is invalid.")
        {
            
        }
    }
}