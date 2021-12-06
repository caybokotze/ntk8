using System.Net;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware
{
    /// <summary>
    /// Throws 403.
    /// </summary>
    public class UserNotAuthorisedExceptionMiddleware : ExceptionHandlerMiddleware<UserNotAuthorisedException>
    {
        public UserNotAuthorisedExceptionMiddleware() 
            : base((int)HttpStatusCode.Forbidden, ErrorMessageGenerator)
        {
        }

        private static string ErrorMessageGenerator(
            UserNotAuthorisedException ex, 
            HttpContext ctx)
        {
            return ex.Message;
        }
    }
}