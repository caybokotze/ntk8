using System;

namespace Ntk8.Exceptions
{
    public class UserIsVerifiedException : Exception
    {
        public UserIsVerifiedException(string? message = null) : base(message ?? "User is already verified.")
        {
            
        }
    }
}