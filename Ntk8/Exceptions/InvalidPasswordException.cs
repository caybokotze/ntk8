using System;

namespace Ntk8.Exceptions
{
    public class InvalidPasswordException : Exception
    {
        public InvalidPasswordException(string? message = null) : base(message ?? "The provided password is incorrect")
        {
            
        }
    }
}