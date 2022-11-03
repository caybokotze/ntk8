using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions;

[Serializable]
public class InvalidEmailAddressException : Exception
{
    protected InvalidEmailAddressException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
    {
    }
    
    public InvalidEmailAddressException(string? message = null) : base(message ?? "Email address is invalid or null")
    {
        
    }
}