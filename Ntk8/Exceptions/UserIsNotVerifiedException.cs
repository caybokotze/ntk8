using System;

namespace Ntk8.Exceptions
{
    public class UserIsNotVerifiedException : Exception
    {
        public UserIsNotVerifiedException() : base("User is not verified")
        {
            
        }
    }
}