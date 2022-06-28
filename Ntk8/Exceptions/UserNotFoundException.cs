using System;

namespace Ntk8.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string? message = null) : base(message ?? "The user does not exist") 
        {
            
        }
    }
}