using System;

namespace Ntk8.Exceptions;

public class InvalidValidationTokenException : Exception
{
    public InvalidValidationTokenException() : base("Invalid validation token. This account has either already been validated or not registered.")
    {
        
    }
}