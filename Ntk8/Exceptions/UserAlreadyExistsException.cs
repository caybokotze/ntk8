using System;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Ntk8.Exceptions
{
    [Serializable]
    public class UserAlreadyExistsException : Exception
    {
        protected UserAlreadyExistsException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
        {
            
        }
        
        public UserAlreadyExistsException(string? message = null) : base(message ?? "User already exists.")
        {
            
        }
    }
}