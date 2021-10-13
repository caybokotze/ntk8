using System.Text;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Demo
{
    public static class HttpContextExtensions
    {
        public static void PrintToWeb(this HttpContext httpContext, string text)
        {
            httpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(text));
        }

        public static void SerialiseAndPrintObject(this HttpContext httpContext, object value)
        {
            var serializedObject = System.Text.Json.JsonSerializer.Serialize(value);
            httpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(serializedObject));
        }
    }
}