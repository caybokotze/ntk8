using System;

namespace Ntk8.Exceptions
{
    public class UserIsNotVerifiedException : Exception
    {
        public UserIsNotVerifiedException(string? message = null) : base(message ?? "User is not verified")
        {
            
        }
    }
}