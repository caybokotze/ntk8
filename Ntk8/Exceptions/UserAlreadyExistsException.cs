using System;

namespace Ntk8.Exceptions
{
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string? message = null) : base(message ?? "User already exists.")
        {
            
        }
    }
}