using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions;

[Serializable]
public class InvalidUserException : Exception
{
    protected InvalidUserException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
    {
        
    }
    
    public InvalidUserException(string? message = null) : base(message ?? "Invalid user information provided. Check that the primary key is not 0")
    {
        
    }
}