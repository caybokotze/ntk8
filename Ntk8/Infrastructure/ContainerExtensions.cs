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
        where TUser : class, IBaseUser, new()
    {
        var ntk8Options = new Ntk8Options<TUser>();
        options?.Invoke(ntk8Options);

        var commandType = ntk8Options.GetNtk8CommandType();
        var queryType = ntk8Options.GetNtk8QueryType();

        if (queryType is null)
        {
            serviceCollection.AddTransient<INtk8Queries<TUser>, Ntk8Queries<TUser>>();
        }

        if (queryType is not null)
        {
            serviceCollection.AddTransient(typeof(INtk8Queries<TUser>), queryType);
        }

        if (commandType is null)
        {
            serviceCollection.AddTransient<INtk8Commands, Ntk8Commands>();
        }

        if (commandType is not null)
        {
            serviceCollection.AddTransient(typeof(INtk8Commands), commandType);
        }

        serviceCollection.AddSingleton<IGlobalSettings, GlobalSettings>(_ => new GlobalSettings
        {
            UseJwt = ntk8Options.UseJwt
        });
        
        serviceCollection.AddSingleton<AuthSettings, AuthSettings>(_ => ntk8Options.GetAuthSettings());
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
    }
    
    public static void RegisterNtk8Services<TUser>(this IServiceCollection serviceCollection) where TUser : class, IBaseUser, new()
    {
        serviceCollection.AddTransient<IBaseUser, TUser>();
        serviceCollection.AddTransient<JwtMiddleware<TUser>, JwtMiddleware<TUser>>();
        serviceCollection.AddTransient<IAccountService, AccountService<TUser>>();
        serviceCollection.AddTransient<ITokenService, TokenService<TUser>>();
        serviceCollection.AddScoped<IAccountState, AccountState>();
    }
}