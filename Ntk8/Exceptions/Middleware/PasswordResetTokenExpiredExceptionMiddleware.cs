using System.Net;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware;

public class PasswordResetTokenExpiredExceptionMiddleware : ExceptionHandlerMiddleware<PasswordResetTokenExpiredException>
{
    public PasswordResetTokenExpiredExceptionMiddleware() : base((int) HttpStatusCode.BadRequest, GenerateMessage)
    {
    }

    private static string GenerateMessage(PasswordResetTokenExpiredException ex, HttpContext context)
    {
        return ex.Message;
    }
}