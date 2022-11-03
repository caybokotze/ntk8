using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions
{
    [Serializable]
    public class UserNotFoundException : Exception
    {
        protected UserNotFoundException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
        {
            
        }
        public UserNotFoundException(string? message = null) : base(message ?? "The user does not exist") 
        {
            
        }
    }
}