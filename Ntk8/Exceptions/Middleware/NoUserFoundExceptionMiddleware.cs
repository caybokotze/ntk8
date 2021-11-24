using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware
{
    public class NoUserFoundExceptionMiddleware: ExceptionHandlerMiddleware<NoUserFoundException>
    {
        public NoUserFoundExceptionMiddleware() : base(400, GenerateMessage)
        {
        }

        private static string GenerateMessage(NoUserFoundException ex, HttpContext context)
        {
            return ex.Message;
        }
    }
}