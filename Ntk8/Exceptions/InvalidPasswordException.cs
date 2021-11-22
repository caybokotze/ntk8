using System;

namespace Ntk8.Exceptions
{
    public class InvalidPasswordException : Exception
    {
        public InvalidPasswordException() : base("The provided password is incorrect")
        {
            
        }
    }
}