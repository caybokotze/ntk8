using System;
using Ntk8.Models;

namespace Ntk8.Exceptions
{
    public class UserNotAuthenticatedException : Exception
    {
        public UserNotAuthenticatedException() : base("User is not authenticated")
        {
            
        }
    }
}