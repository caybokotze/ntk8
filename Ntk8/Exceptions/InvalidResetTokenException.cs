using System;

namespace Ntk8.Exceptions
{
    public class InvalidResetTokenException : Exception
    {
        public InvalidResetTokenException() : base("The reset token has expired or is invalid.")
        {
            
        }
    }
}