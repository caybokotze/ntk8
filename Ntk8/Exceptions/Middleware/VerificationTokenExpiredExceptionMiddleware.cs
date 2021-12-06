using System.Net;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware
{
    public class VerificationTokenExpiredExceptionMiddleware: ExceptionHandlerMiddleware<VerificationTokenExpiredException>
    {
        public VerificationTokenExpiredExceptionMiddleware() : base((int)HttpStatusCode.Unauthorized, GenerateMessage)
        {
        }

        private static string GenerateMessage(VerificationTokenExpiredException ex, HttpContext context)
        {
            return ex.Message;
        }
    }
}