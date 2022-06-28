using System;
using Ntk8.DatabaseServices;
using Ntk8.Models;

namespace Ntk8.Infrastructure;

public class Ntk8Options<TUser> : IGlobalSettings where TUser : class, IBaseUser, new()
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
    
    public void OverrideNtk8Commands<TImplementation>() 
        where TImplementation : class, INtk8Queries<TUser>
    {
        Ntk8CommandType = typeof(TImplementation);
    }
        
    public void OverrideNtk8Queries<TImplementation>() 
        where TImplementation : class, INtk8Queries<TUser>
    {
        Ntk8QueryType = typeof(TImplementation);
    }
}