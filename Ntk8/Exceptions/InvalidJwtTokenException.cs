using System;

namespace Ntk8.Exceptions
{
    public class InvalidJwtTokenException : Exception
    {
        public InvalidJwtTokenException() : base("The jwt token has expired or is invalid.")
        {
            
        }
    }
}