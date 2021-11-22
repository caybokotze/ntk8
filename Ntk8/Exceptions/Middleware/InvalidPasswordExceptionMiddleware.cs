using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware
{
    public class InvalidPasswordExceptionMiddleware : ExceptionHandlerMiddleware<InvalidPasswordException>
    {
        public InvalidPasswordExceptionMiddleware() : base(401, GenerateMessage)
        {
        }

        private static string GenerateMessage(InvalidPasswordException ex, HttpContext context)
        {
            return ex.Message;
        }
    }
}