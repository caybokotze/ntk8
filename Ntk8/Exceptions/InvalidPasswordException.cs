using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions
{
    [Serializable]
    public class InvalidPasswordException : Exception
    {
        protected InvalidPasswordException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
        {
            
        }
        
        public InvalidPasswordException(string? message = null) : base(message ?? "The provided password is incorrect")
        {
            
        }
    }
}