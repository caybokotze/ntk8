using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Exceptions.Middleware;

public class InvalidDataExceptionMiddleware : ExceptionHandlerMiddleware<InvalidDataException>
{
    public InvalidDataExceptionMiddleware() : base(StatusCodes.Status400BadRequest, GenerateMessage)
    {
    }
    
    private static string GenerateMessage(InvalidDataException ex, HttpContext context)
    {
        return ex.Message;
    }
}