using System;

namespace Ntk8.Exceptions
{
    public class UserNotAuthorisedException : Exception
    {
        public UserNotAuthorisedException(string? message = null) : base(message ?? "User not authorised.")
        {
            
        }
    }
}