using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware
{
    /// <summary>
    /// Throws 401
    /// </summary>
    public class UserNotAuthenticatedExceptionMiddleware : ExceptionHandlerMiddleware<UserNotAuthenticatedException>
    {
        public UserNotAuthenticatedExceptionMiddleware() 
            : base(401, ErrorMessageGenerator)
        {
            
        }

        private static string ErrorMessageGenerator(UserNotAuthenticatedException ex, HttpContext context)
        {
            return ex.Message;
        }
    }
}