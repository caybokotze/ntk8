using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ntk8.Exceptions.Middleware;
using Ntk8.Middleware;
using Ntk8.Models;
using Ntk8.Services;

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

        public static IServiceCollection RegisterNtk8AuthenticationServices(
            this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<IAuthenticationContextService, AuthenticationContextService>();
            serviceCollection.AddTransient<IUserAccountService, UserAccountService>();
            serviceCollection.AddTransient<ITokenService, TokenService>();
            serviceCollection.AddTransient<JwtMiddleware>();
            return serviceCollection;
        }

        public static IServiceCollection RegisterAndConfigureNtk8AuthenticationSettings(
            this IServiceCollection serviceCollection,
            IConfiguration configuration, string jsonSettingName = null)
        {
            serviceCollection
                .AddSingleton<IAuthSettings, AuthSettings>(sp => configuration
                    .GetSection(jsonSettingName ?? "AuthSettings")
                    .Get<AuthSettings>());
            return serviceCollection;
        }

        public static IServiceCollection RegisterNtk8JwtMiddleware(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<JwtMiddleware>();
            return serviceCollection;
        }
    }
}