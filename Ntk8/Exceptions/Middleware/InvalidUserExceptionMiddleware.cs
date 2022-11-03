using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware;

public class InvalidUserExceptionMiddleware : ExceptionHandlerMiddleware<InvalidUserException>
{
    public InvalidUserExceptionMiddleware() : base(StatusCodes.Status400BadRequest, GenerateMessage)
    {
    }

    private static string GenerateMessage(InvalidUserException ex, HttpContext context)
    {
        return ex.Message;
    }
}