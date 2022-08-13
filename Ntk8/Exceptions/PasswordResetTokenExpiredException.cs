using System;
using System.Runtime.Serialization;

namespace Ntk8.Exceptions;

[Serializable]
public class PasswordResetTokenExpiredException : Exception
{
    protected PasswordResetTokenExpiredException(SerializationInfo info, StreamingContext streamingContext) : base(info,
        streamingContext)
    {
        
    }
    
    public PasswordResetTokenExpiredException(string? message = null) : base(message ?? "Password reset token has expired")
    {
        
    }
}