using Microsoft.Extensions.DependencyInjection;
using Ntk8.Exceptions.Middleware;

namespace Ntk8.Helpers
{
    public static class MiddlewareExtensions
    {
        public static IServiceCollection RegisterNtk8MiddlewareExceptionHandlers(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<UserNotAuthorisedExceptionMiddleware, UserNotAuthorisedExceptionMiddleware>();
            serviceCollection
                .AddSingleton<UserNotAuthenticatedExceptionMiddleware, UserNotAuthenticatedExceptionMiddleware>();
            return serviceCollection;
        }

        public static IServiceCollection RegisterNtk8AuthenticationServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection;
        }

        public static IServiceCollection RegisterNtk8JwtMiddleware(this IServiceCollection serviceCollection)
        {
            return serviceCollection;
        }
    }
}