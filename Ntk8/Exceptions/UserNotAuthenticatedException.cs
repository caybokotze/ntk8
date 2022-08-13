using System;

namespace Ntk8.Exceptions
{
    public class UserNotAuthenticatedException : Exception
    {
        public UserNotAuthenticatedException(string? message = null) : base(message ?? "User is not authenticated")
        {
            
        }
    }
}