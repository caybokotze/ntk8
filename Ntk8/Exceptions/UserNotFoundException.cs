using System;

namespace Ntk8.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException() : base("The user does not exist") 
        {
            
        }
    }
}