using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions
{
    [Serializable]
    public class InvalidTokenLengthException : Exception
    {
        protected InvalidTokenLengthException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
        {
            
        }
        
        public InvalidTokenLengthException(string? message = null) : base(message ?? "Invalid token length. The token secret must be at least 32 characters long to meet AES 128 standards.")
        {
            
        }
    }
}