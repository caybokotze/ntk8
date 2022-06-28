using System;

namespace Ntk8.Exceptions
{
    public class InvalidResetTokenException : Exception
    {
        public InvalidResetTokenException(string? message = null) : base(message ?? "The reset token has expired or is invalid.")
        {
            
        }
    }
}