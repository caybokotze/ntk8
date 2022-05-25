using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ntk8.Exceptions;
using Ntk8.Exceptions.Middleware;
using Ntk8.Middleware;
using Ntk8.Models;
using Ntk8.Services;

namespace Ntk8.Infrastructure;

public static class ContainerExtensions
{
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
    
    public static void RegisterNtk8Services<T>(this IServiceCollection serviceCollection) where T : class, IBaseUser, new()
    {
        serviceCollection.AddTransient<IBaseUser, T>();
        serviceCollection.AddTransient<JwtMiddleware<T>, JwtMiddleware<T>>();
        serviceCollection.AddTransient<IAccountService, AccountService<T>>();
        serviceCollection.AddTransient<ITokenService, TokenService<T>>();
        serviceCollection.AddScoped<IAccountState, AccountState>();
    }
    
    public static void ConfigureNkt8Settings(this IServiceCollection serviceCollection,
        IConfiguration configuration, string? jsonSettingName = null)
    {
        var authSettings = configuration
            .GetSection(jsonSettingName ?? "AuthSettings")
            .Get<AuthSettings>();

        if (authSettings is null)
        {
            throw new KeyNotFoundException("The authentication settings can not be found in the specified appsettings.json file.");
        }

        if (authSettings.RefreshTokenSecret?.Length < 32)
        {
            throw new InvalidTokenLengthException();
        }
            
        serviceCollection
            .AddSingleton<IAuthSettings, AuthSettings>(_ => authSettings);
    }
}