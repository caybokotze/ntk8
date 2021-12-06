using System.Net;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware
{
    public class InvalidRefreshTokenExceptionMiddleware : ExceptionHandlerMiddleware<InvalidRefreshTokenException>
    {
        public InvalidRefreshTokenExceptionMiddleware() : base((int)HttpStatusCode.Unauthorized, GenerateMessage)
        {
            
        }
        
        private static string GenerateMessage(InvalidRefreshTokenException ex, HttpContext context)
        {
            return ex.Message;
        }
    }
}