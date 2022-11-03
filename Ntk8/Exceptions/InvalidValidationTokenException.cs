using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions;

[Serializable]
public class InvalidValidationTokenException : Exception
{
    protected InvalidValidationTokenException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
    {
        
    }
    
    public InvalidValidationTokenException(string? message = null) : base(message ?? "Invalid validation token. This account has either already been validated or not registered.")
    {
        
    }
}