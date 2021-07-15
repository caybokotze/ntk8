using System;

namespace Ntk8.Exceptions
{
    public class NoUserFoundException : Exception
    {
        public NoUserFoundException() : base("No user found")
        {
            
        }
    }
}