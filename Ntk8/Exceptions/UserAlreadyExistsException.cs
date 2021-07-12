using System;

namespace Ntk8.Exceptions
{
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException() : base("User already exists.")
        {
            
        }
    }
}