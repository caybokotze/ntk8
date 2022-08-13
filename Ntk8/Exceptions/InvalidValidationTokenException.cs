using System;

namespace Ntk8.Exceptions;

public class InvalidValidationTokenException : Exception
{
    public InvalidValidationTokenException(string? message = null) : base(message ?? "Invalid validation token. This account has either already been validated or not registered.")
    {
        
    }
}