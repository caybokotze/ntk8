using System.Net;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware
{
    /// <summary>
    /// Throws 401
    /// </summary>
    public class UserNotAuthenticatedExceptionMiddleware : ExceptionHandlerMiddleware<UserNotAuthenticatedException>
    {
        public UserNotAuthenticatedExceptionMiddleware() 
            : base((int)HttpStatusCode.Unauthorized, ErrorMessageGenerator)
        {
            
        }

        private static string ErrorMessageGenerator(UserNotAuthenticatedException ex, HttpContext context)
        {
            return ex.Message;
        }
    }
}