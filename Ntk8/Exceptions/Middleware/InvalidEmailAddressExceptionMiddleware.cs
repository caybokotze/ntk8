using System.Net;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware;

public class InvalidEmailAddressExceptionMiddleware : ExceptionHandlerMiddleware<InvalidEmailAddressException>
{
    public InvalidEmailAddressExceptionMiddleware() : base((int) HttpStatusCode.BadRequest, GenerateMessage)
    {
    }

    private static string GenerateMessage(InvalidEmailAddressException ex, HttpContext context)
    {
        return ex.Message;
    }
}