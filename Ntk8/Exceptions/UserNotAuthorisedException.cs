using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions
{
    [Serializable]
    public class UserNotAuthorisedException : Exception
    {
        protected UserNotAuthorisedException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
        {
            
        }
        
        public UserNotAuthorisedException(string? message = null) : base(message ?? "User not authorised.")
        {
            
        }
    }
}