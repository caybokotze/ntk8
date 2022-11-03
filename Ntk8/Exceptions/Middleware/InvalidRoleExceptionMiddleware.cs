using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware;

public class InvalidRoleExceptionMiddleware : ExceptionHandlerMiddleware<InvalidRoleException>
{
    public InvalidRoleExceptionMiddleware() : base(StatusCodes.Status400BadRequest, GenerateMessage)
    {
    }

    private static string GenerateMessage(InvalidRoleException ex, HttpContext context)
    {
        return ex.Message;
    }
}