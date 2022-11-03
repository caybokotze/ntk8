using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions;

[Serializable]
public class InvalidVerificationTokenException : Exception
{
    protected InvalidVerificationTokenException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
    {
        
    }
    
    public InvalidVerificationTokenException(string? message = null) : base(message ?? "Invalid validation token. This account has either already been validated or not registered.")
    {
        
    }
}