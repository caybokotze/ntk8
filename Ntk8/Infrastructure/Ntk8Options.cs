using System;
using Ntk8.DatabaseServices;
using Ntk8.Exceptions;
using Ntk8.Models;

namespace Ntk8.Infrastructure;

public class Ntk8Options<TUser> : IGlobalSettings where TUser : class, IUserEntity, new()
{
    public Ntk8Options()
    {
        AuthSettings = new AuthSettings();
    }
    
    public bool UseJwt { get; set; }
    private Type? Ntk8QueryType { get; set; }
    private Type? Ntk8CommandType { get; set; }
    private AuthSettings AuthSettings { get; set; }

    public Type? GetNtk8QueryType()
    {
        return Ntk8QueryType;
    }
    
    public Type? GetNtk8CommandType()
    {
        return Ntk8CommandType;
    }

    public AuthSettings GetAuthSettings()
    {
        return AuthSettings;
    }

    public void ConfigureAuthSettings(Action<AuthSettings> configureAuthSettings)
    {
        var authSettings = new AuthSettings();
        configureAuthSettings.Invoke(authSettings);
        if (authSettings.RefreshTokenSecret is null or "")
        {
            throw new InvalidRefreshTokenException(@"A refresh token needs to be specified in the application service registration container.
            Example: 
            builder.Services.ConfigureNtk8<UserEntity>(o =>
            {
                o.UseJwt = true;
                o.ConfigureAuthSettings(a =>
                {
                    a.RefreshTokenSecret = // insert refresh token secret here
                    a.JwtTTL = 30_000;
                    a.UserVerificationTokenTTL = 10_000;
                });
            });");
        }
        AuthSettings = authSettings;
    }
    
    public void OverrideUserCommands<TImplementation>() 
        where TImplementation : class, IAccountCommands
    {
        Ntk8CommandType = typeof(TImplementation);
    }
        
    public void OverrideUserQueries<TImplementation>() 
        where TImplementation : class, IAccountQueries
    {
        Ntk8QueryType = typeof(TImplementation);
    }
}