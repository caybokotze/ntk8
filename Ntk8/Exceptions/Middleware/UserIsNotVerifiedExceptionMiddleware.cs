using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware
{
    public class UserIsNotVerifiedExceptionMiddleware : ExceptionHandlerMiddleware<UserIsNotVerifiedException>
    {
        public UserIsNotVerifiedExceptionMiddleware() : base(400, GenerateMessage)
        {
        }

        private static string GenerateMessage(UserIsNotVerifiedException ex, HttpContext context)
        {
            return ex.Message;
        }
    }
}