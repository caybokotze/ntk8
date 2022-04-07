using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ntk8.Helpers;

public class Ntk8CustomSqlStatements
{
    public string? FetchUserByVerificationTokenStatement { get; set; }
    public string? FetchUserByEmailAddressStatement { get; set; }
    public string? FetchUserByIdStatement { get; set; }
    public string? FetchUserByRefreshTokenStatement { get; set; }
    public string? FetchUserByResetTokenStatement { get; set; }
    public string? UpdateUserStatement { get; set; }
    public string? InsertUserStatement { get; set; }
}

public static class Ntk8ConfigurationHelpers
{
    public static void ConfigureNtk8CustomSql(this IServiceCollection serviceCollection, Action<Ntk8CustomSqlStatements> configuration)
    {
        var ntk8Configuration = new Ntk8CustomSqlStatements();
        configuration(ntk8Configuration);
        serviceCollection.AddSingleton<Ntk8CustomSqlStatements, Ntk8CustomSqlStatements>(_ => ntk8Configuration);
    }
}