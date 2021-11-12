﻿using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Dapper.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySql.Data.MySqlClient;
using Ntk8.Middleware;
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
            // builder.Services.AddCors();
            builder = ConfigureDependencies(builder);
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            var app = builder.Build();
            var _ = new AuthHandler(app, app.Resolve<IUserAccountService>(),
                app.Resolve<IAuthenticationContextService>());
            app.UseMiddleware<JwtMiddleware>();
            // app.UseCors();
            app.Run();
        }
        
        // TODO: Build middleware that will dispose the refresh token once it has been set.
        // TODO: Make sure that fetching a user and user_roles is optimized to one database call.
        // TODO: When fetching a user, also attach the most recent token to that user.
        // TODO: Setup the middleware to handle exceptions and return 403, 401's appropriately.
        // TODO: Write custom authorise attribute to handle role management.
        // TODO: When saving new users, isActive should be set to true.
        // TODO: When deleting a user, isActive should be set to false.
        // TODO: When fetching users, isActive results set to false, should be ignored.

        private static WebApplicationBuilder ConfigureDependencies(WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IQueryExecutor, QueryExecutor>();
            builder.Services.AddTransient<ICommandExecutor, CommandExecutor>();
            builder.Services.AddTransient<IDbConnection, DbConnection>(sp => new MySqlConnection(GetConnectionString()));
            builder.Services.AddHttpContextAccessor();
            builder.Services.TryAddSingleton<IAuthenticationContextService, AuthenticationContextService>();
            builder.Services.AddTransient<IUserAccountService, UserAccountService>();
            builder.Services.AddTransient<IAuthSettings, AuthSettings>(sp => ResolveAuthSettings());
            builder.Services.AddTransient<ITokenService, TokenService>();
            builder.Services.AddTransient<JwtMiddleware>();
            return builder;
        }

        private static AuthSettings ResolveAuthSettings()
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