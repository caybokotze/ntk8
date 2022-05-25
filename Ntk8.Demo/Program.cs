﻿using System.Data;
using System.Data.Common;
using Dapper.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Ntk8.Helpers;
using Ntk8.Infrastructure;
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
            ConfigureDependencies(builder);

            builder.Services.ConfigureNtk8CustomSql(c =>
            {
                
            });
            
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            var app = builder.Build();
            app.UseRouting();
            app.UseNtk8JwtMiddleware<User>();
            app.UseNtk8ExceptionMiddleware();
            
            var _ = new AuthHandler(app, app.Resolve<IAccountService>(),
                app.Resolve<IAuthenticationContextService>(),
                app.Resolve<IQueryExecutor>(),
                app.Resolve<ICommandExecutor>(),
                app.Resolve<ITokenService>(),
                app.Resolve<IHttpContextAccessor>());
           
            app.Run();
        }

        // TODO: See if we can leverage the existing dotnet Authorise Attribute to use within the Ntk8 package.
        // TODO: When saving new users, isActive should be set to true.
        // TODO: When deleting a user, isActive should be set to false.
        // TODO: When fetching users, isActive results set to false, should be ignored.
        // TODO: Salt the password before storing.
        // TODO: Fix the authenticated response to not contain dates.
        // TODO: The UserContextService needs to return the current logged in user.

        private static void ConfigureDependencies(WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IQueryExecutor, QueryExecutor>();
            builder.Services.AddTransient<ICommandExecutor, CommandExecutor>();
            builder.Services.AddTransient<IDbConnection, DbConnection>(sp => new MySqlConnection(GetConnectionString()));
            builder.Services.AddHttpContextAccessor();
            builder.Services.RegisterNtk8MiddlewareExceptionHandlers();
            builder.Services.RegisterNtk8Services<User>();
            builder.Services.ConfigureNkt8Settings(CreateConfigurationRoot());
        }

        public static string GetConnectionString()
        {
            var config = CreateConfigurationRoot()
                .GetConnectionString("DefaultConnection");

            return config;
        }
    }
}