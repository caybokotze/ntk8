using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions
{
    [Serializable]
    public class InvalidResetTokenException : Exception
    {
        protected InvalidResetTokenException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
        {
            
        }
        
        public InvalidResetTokenException(string? message = null) : base(message ?? "The reset token has expired or is invalid.")
        {
            
        }
    }
}