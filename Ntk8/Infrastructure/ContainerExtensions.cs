using System;
using Microsoft.Extensions.DependencyInjection;
using Ntk8.DatabaseServices;
using Ntk8.Exceptions.Middleware;
using Ntk8.Middleware;
using Ntk8.Models;
using Ntk8.Services;

namespace Ntk8.Infrastructure;

public static class ContainerExtensions
{
    public static void ConfigureNtk8<TUser>(
        this IServiceCollection serviceCollection,
        Action<Ntk8Options<TUser>>? options = null)
        where TUser : 
        class, 
        IUserEntity, 
        new()
    {
        var ntk8Options = new Ntk8Options<TUser>();
        options?.Invoke(ntk8Options);
        var commandType = ntk8Options.GetNtk8CommandType();
        var queryType = ntk8Options.GetNtk8QueryType();

        if (queryType is null)
        {
            serviceCollection.AddTransient<IAccountQueries, AccountQueries>();
        }

        if (queryType is not null)
        {
            serviceCollection.AddTransient(typeof(IAccountQueries), queryType);
        }

        if (commandType is null)
        {
            serviceCollection.AddTransient<IAccountCommands, AccountCommands>();
        }

        if (commandType is not null)
        {
            serviceCollection.AddTransient(typeof(IAccountCommands), commandType);
        }

        serviceCollection.AddSingleton<IGlobalSettings, GlobalSettings>(_ => new GlobalSettings
        {
            UseJwt = ntk8Options.UseJwt
        });
        
        serviceCollection.AddSingleton<IAuthSettings, AuthSettings>(_ => ntk8Options.GetAuthSettings());
        RegisterNtk8Services<TUser>(serviceCollection);
        RegisterNtk8ExceptionHandlers(serviceCollection);
    }
    
    public static void RegisterNtk8ExceptionHandlers(this IServiceCollection serviceCollection)
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
            .AddSingleton<VerificationTokenExpiredExceptionMiddleware, VerificationTokenExpiredExceptionMiddleware>();
        serviceCollection
            .AddSingleton<InvalidRefreshTokenExceptionMiddleware, InvalidRefreshTokenExceptionMiddleware>();
        serviceCollection
            .AddSingleton<InvalidJwtTokenExceptionMiddleware, InvalidJwtTokenExceptionMiddleware>();
        serviceCollection
            .AddSingleton<InvalidEmailAddressExceptionMiddleware, InvalidEmailAddressExceptionMiddleware>();
        serviceCollection
            .AddSingleton<PasswordResetTokenExpiredExceptionMiddleware, PasswordResetTokenExpiredExceptionMiddleware>();
        serviceCollection
            .AddSingleton<InvalidUserExceptionMiddleware, InvalidUserExceptionMiddleware>();
        serviceCollection
            .AddSingleton<InvalidRoleExceptionMiddleware, InvalidRoleExceptionMiddleware>();
    }
    
    public static void RegisterNtk8Services<T>(
        this IServiceCollection serviceCollection) 
        where T : class, 
        IUserEntity, 
        new()
    {
        serviceCollection.AddScoped<JwtMiddleware<T>, JwtMiddleware<T>>();
        serviceCollection.AddScoped<ITokenService, TokenService<T>>();
        serviceCollection.AddScoped<IAccountService, AccountService<T>>();
    }
}