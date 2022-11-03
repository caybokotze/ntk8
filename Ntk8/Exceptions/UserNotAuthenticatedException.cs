using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions
{
    [Serializable]
    public class UserNotAuthenticatedException : Exception
    {
        protected UserNotAuthenticatedException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
        {
            
        }
        
        public UserNotAuthenticatedException(string? message = null) : base(message ?? "User is not authenticated")
        {
            
        }
    }
}