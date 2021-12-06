using System.Net;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware
{
    public class UserNotFoundExceptionMiddleware: ExceptionHandlerMiddleware<UserNotFoundException>
    {
        public UserNotFoundExceptionMiddleware() : base((int)HttpStatusCode.BadRequest, GenerateMessage)
        {
        }

        private static string GenerateMessage(UserNotFoundException ex, HttpContext context)
        {
            return ex.Message;
        }
    }
}