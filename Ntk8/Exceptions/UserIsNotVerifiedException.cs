using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions
{
    [Serializable]
    public class UserIsNotVerifiedException : Exception
    {
        protected UserIsNotVerifiedException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
        {
            
        }
        
        public UserIsNotVerifiedException(string? message = null) : base(message ?? "User is not verified")
        {
            
        }
    }
}