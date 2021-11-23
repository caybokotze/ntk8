using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware
{
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