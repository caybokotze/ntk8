using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions
{
    [Serializable]
    public class InvalidJwtTokenException : Exception
    {
        protected InvalidJwtTokenException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
        {
            
        }
        
        public InvalidJwtTokenException(string? message = null) : base(message ?? "The jwt token has expired or is invalid.")
        {
            
        }
    }
}