using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Ntk8.Demo
{
    public static class GlobalExtensions
    {
        public static T Resolve<T>(this WebApplication webApplication)
        {
            return webApplication.Services.GetRequiredService<T>();
        }
        
        public static T Resolve<T>(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            return endpointRouteBuilder
                .ServiceProvider
                .GetRequiredService<T>();
        }
    }
}