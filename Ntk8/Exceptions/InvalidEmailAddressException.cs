using System;

namespace Ntk8.Exceptions;

public class InvalidEmailAddressException : Exception
{
    public InvalidEmailAddressException(string? message = null) : base(message ?? "Invalid email address")
    {
        
    }
}