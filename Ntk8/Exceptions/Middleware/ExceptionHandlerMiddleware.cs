using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ntk8.Exceptions.Middleware
{
    public class ExceptionHandlerMiddleware<T> : IMiddleware where T : Exception
    {
        private readonly int _errorCode;
        private readonly Func<T, HttpContext, string> _errorMessageGenerator;

        public ExceptionHandlerMiddleware(
            int errorCode,
            string errorMessage) : this(errorCode, (_ , _) => errorMessage)
        {
        }

        public ExceptionHandlerMiddleware(int errorCode, Func<T, HttpContext, string> errorMessageGenerator)
        {
            _errorCode = errorCode;
            _errorMessageGenerator = errorMessageGenerator;
        }
        
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (T ex)
            {
                context.Response.StatusCode = _errorCode;
                var content = _errorMessageGenerator?.Invoke(ex, context);
                if (string.IsNullOrWhiteSpace(content))
                {
                    return;
                }

                var asBytes = Encoding.UTF8.GetBytes(content);
                await context.Response.Body.WriteAsync(asBytes, 0, asBytes.Length);
                TryLogException(context, ex);
            }
        }

        private static void TryLogException(HttpContext context, T ex)
        {
            try
            {
                var logger = context.RequestServices.GetService<ILogger<T>>();
                logger.LogError(ex, ex.Message);
            }
            catch
            {
                // ignore.
            }
        }
    }
}