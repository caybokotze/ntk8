using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PeanutButter.Utils;

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

        public static async Task<T> DeserializeRequestBody<T>(this HttpContext httpContext)
        {
            return JsonConvert
                .DeserializeObject<T>(await new StreamReader(httpContext.Request.Body)
                    .ReadToEndAsync());
        }
        
        public static async Task SerialiseResponseBody(this HttpContext httpContext, object response)
        {
            var payload = JsonConvert.SerializeObject(response);
            await httpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(payload));
        }

 
    }
}