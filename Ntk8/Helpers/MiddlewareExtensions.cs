using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ntk8.Exceptions;
using Ntk8.Exceptions.Middleware;
using Ntk8.Middleware;
using Ntk8.Models;
using Ntk8.Services;

namespace Ntk8.Helpers
{
    public static class MiddlewareExtensions
    {
        public static void UseNtk8JwtMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<JwtMiddleware>();
        }

        public static void UseNtk8ExceptionMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<UserNotAuthenticatedExceptionMiddleware>();
            builder.UseMiddleware<UserNotAuthorisedExceptionMiddleware>();
            builder.UseMiddleware<InvalidPasswordExceptionMiddleware>();
            builder.UseMiddleware<UserNotFoundExceptionMiddleware>();
            builder.UseMiddleware<UserAlreadyExistsExceptionMiddleware>();
            builder.UseMiddleware<UserIsNotVerifiedExceptionMiddleware>();
            builder.UseMiddleware<UserIsVerifiedExceptionMiddleware>();
            builder.UseMiddleware<VerificationTokenExpiredExceptionMiddleware>();
            builder.UseMiddleware<InvalidRefreshTokenExceptionMiddleware>();
            builder.UseMiddleware<InvalidJwtTokenExceptionMiddleware>();
        }
        
        public static void RegisterNtk8MiddlewareExceptionHandlers(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<UserNotAuthorisedExceptionMiddleware, UserNotAuthorisedExceptionMiddleware>();
            serviceCollection
                .AddSingleton<UserNotAuthenticatedExceptionMiddleware, UserNotAuthenticatedExceptionMiddleware>();
            serviceCollection
                .AddSingleton<InvalidPasswordExceptionMiddleware, InvalidPasswordExceptionMiddleware>();
            serviceCollection
                .AddSingleton<UserNotFoundExceptionMiddleware, UserNotFoundExceptionMiddleware>();
            serviceCollection
                .AddSingleton<UserAlreadyExistsExceptionMiddleware, UserAlreadyExistsExceptionMiddleware>();
            serviceCollection
                .AddSingleton<UserIsNotVerifiedExceptionMiddleware, UserIsNotVerifiedExceptionMiddleware>();
            serviceCollection
                .AddSingleton<UserIsVerifiedExceptionMiddleware, UserIsVerifiedExceptionMiddleware>();
            serviceCollection
                .AddSingleton<VerificationTokenExpiredExceptionMiddleware,
                    VerificationTokenExpiredExceptionMiddleware>();
            serviceCollection
                .AddSingleton<InvalidRefreshTokenExceptionMiddleware, InvalidRefreshTokenExceptionMiddleware>();
            serviceCollection.AddSingleton<InvalidJwtTokenExceptionMiddleware, InvalidJwtTokenExceptionMiddleware>();
        }

        public static void RegisterNtk8AuthenticationServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<IAuthenticationContextService, AuthenticationContextService>();
            serviceCollection.AddTransient<IUserAccountService, UserAccountService>();
            serviceCollection.AddTransient<ITokenService, TokenService>();
            serviceCollection.AddTransient<JwtMiddleware>();
        }

        public static void RegisterAndConfigureNtk8AuthenticationSettings(this IServiceCollection serviceCollection,
            IConfiguration configuration, string jsonSettingName = null)
        {
            var authSettings = configuration
                .GetSection(jsonSettingName ?? "AuthSettings")
                .Get<AuthSettings>();

            if (authSettings is null)
            {
                throw new KeyNotFoundException("The authentication settings can not be found in the specified appsettings.json file.");
            }

            if (authSettings.RefreshTokenSecret.Length < 32)
            {
                throw new InvalidTokenLengthException();
            }
            
            serviceCollection
                .AddSingleton<IAuthSettings, AuthSettings>(sp => authSettings);
        }
    }
}