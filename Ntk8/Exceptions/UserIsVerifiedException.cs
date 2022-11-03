using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions
{
    [Serializable]
    public class UserIsVerifiedException : Exception
    {
        protected UserIsVerifiedException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
        {
            
        }
        
        public UserIsVerifiedException(string? message = null) : base(message ?? "User is already verified.")
        {
            
        }
    }
}