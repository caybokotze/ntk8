using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Dapper.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySql.Data.MySqlClient;
using Ntk8.Helpers;
using Ntk8.Models;
using Ntk8.Services;
using static ScopeFunction.Utils.AppSettingsBuilder;

namespace Ntk8.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder = ConfigureDependencies(builder);
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            var app = builder.Build();
            var _ = new AuthHandler(app,
                app.Resolve<IAccountService>(),
                app.Resolve<IAuthenticationContextService>());
            app.Run();
        }

        private static WebApplicationBuilder ConfigureDependencies(WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IQueryExecutor, QueryExecutor>();
            builder.Services.AddTransient<ICommandExecutor, CommandExecutor>();
            builder.Services.AddTransient<IDbConnection, DbConnection>(
                sp => new MySqlConnection(GetConnectionString()));
            builder.Services.AddHttpContextAccessor();
            builder.Services.TryAddSingleton<IAuthenticationContextService, AuthenticationContextService>();
            builder.Services.AddTransient<IAccountService, AccountService>();
            builder.Services.AddTransient<IAuthSettings, AuthSettings>(sp => ResolveAuthSettings());
            builder.Services.AddTransient<ITokenService, TokenService>();
            return builder;
        }

        public static AuthSettings ResolveAuthSettings()
        {
            var authSettings = CreateConfigurationRoot()
                .GetSection("AuthSettings")
                .Get<AuthSettings>();
            
            if (authSettings is null)
            {
                throw new KeyNotFoundException("The key specified can not be found in the appsettings.json file.");
            }

            return authSettings;
        }

        public static string GetConnectionString()
        {
            var config = CreateConfigurationRoot()
                .GetConnectionString("DefaultConnection");

            return config;
        }
    }
}