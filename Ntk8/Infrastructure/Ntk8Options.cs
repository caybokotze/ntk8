using System;
using Ntk8.DatabaseServices;
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