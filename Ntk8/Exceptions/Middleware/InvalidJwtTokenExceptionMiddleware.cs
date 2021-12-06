using System.Net;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware
{
    public class InvalidJwtTokenExceptionMiddleware : ExceptionHandlerMiddleware<InvalidJwtTokenException>
    {
        public InvalidJwtTokenExceptionMiddleware() : base((int)HttpStatusCode.Unauthorized, GenerateMessage)
        {
        }

        private static string GenerateMessage(InvalidJwtTokenException ex, HttpContext context)
        {
            return ex.Message;
        }
    }
}