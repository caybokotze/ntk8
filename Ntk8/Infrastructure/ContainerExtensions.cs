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
            serviceCollection.AddTransient<IUserQueries, UserQueries>();
        }

        if (queryType is not null)
        {
            serviceCollection.AddTransient(typeof(IUserQueries), queryType);
        }

        if (commandType is null)
        {
            serviceCollection.AddTransient<IUserCommands, UserCommands>();
        }

        if (commandType is not null)
        {
            serviceCollection.AddTransient(typeof(IUserCommands), commandType);
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
    }
    
    public static void RegisterNtk8Services<TUser>(
        this IServiceCollection serviceCollection) 
        where TUser : class, 
        IUserEntity, 
        new()
    {
        serviceCollection.AddTransient<IUserEntity, TUser>();
        serviceCollection.AddTransient<JwtMiddleware<TUser>, JwtMiddleware<TUser>>();
        serviceCollection.AddTransient<ITokenService, TokenService<TUser>>();
        serviceCollection.AddTransient<IAccountService, AccountService<TUser>>();
    }
}