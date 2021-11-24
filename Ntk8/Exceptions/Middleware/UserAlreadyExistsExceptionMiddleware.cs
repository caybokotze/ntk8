using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware
{
    /// <summary>
    /// Recommended to re-catch exception (with response 200 OK) if you want to prevent email enumeration attacks.
    /// </summary>
    public class UserAlreadyExistsExceptionMiddleware: ExceptionHandlerMiddleware<UserAlreadyExistsException>
    {
        public UserAlreadyExistsExceptionMiddleware() : base(400, GenerateMessage)
        {
        }

        private static string GenerateMessage(UserAlreadyExistsException ex, HttpContext context)
        {
            return ex.Message;
        }
    }
}