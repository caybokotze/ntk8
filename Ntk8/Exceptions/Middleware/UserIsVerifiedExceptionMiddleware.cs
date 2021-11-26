using System.Net;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware
{
    /// <summary>
    /// Returns bad request - 400
    /// </summary>
    public class UserIsVerifiedExceptionMiddleware: ExceptionHandlerMiddleware<UserIsVerifiedException>
    {
        public UserIsVerifiedExceptionMiddleware() : base((int)HttpStatusCode.BadRequest, GenerateMessage)
        {
        }

        private static string GenerateMessage(UserIsVerifiedException ex, HttpContext context)
        {
            return ex.Message;
        }
    }
}