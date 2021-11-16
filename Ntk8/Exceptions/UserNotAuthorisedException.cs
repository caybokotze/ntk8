using System;

namespace Ntk8.Exceptions
{
    public class UserNotAuthorisedException : Exception
    {
        public UserNotAuthorisedException() : base("Not authorised.")
        {
            
        }
    }
}