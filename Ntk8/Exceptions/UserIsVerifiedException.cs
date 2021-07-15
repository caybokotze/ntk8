using System;

namespace Ntk8.Exceptions
{
    public class UserIsVerifiedException : Exception
    {
        public UserIsVerifiedException() : base("User is already verified.")
        {
            
        }
    }
}