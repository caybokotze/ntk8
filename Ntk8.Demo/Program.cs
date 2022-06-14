using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Ntk8.Infrastructure;
using Ntk8.Services;
using static ScopeFunction.Utils.AppSettingsBuilder;

namespace Ntk8.Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseDefaultServiceProvider(options => options.ValidateScopes = true);
            ConfigureDependencies(builder);

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            var app = builder.Build();
            app.UseRouting();
            app.UseNtk8JwtMiddleware<User>();
            app.UseNtk8ExceptionMiddleware();
            
            var _ = new AuthHandler(app, 
                app.Resolve<IAccountService>(),
                app.Resolve<IQueryExecutor>(),
                app.Resolve<ICommandExecutor>(),
                app.Resolve<ITokenService>(),
                app.Resolve<IHttpContextAccessor>());
            
            await app.RunAsync();
        }
        
        // TODO: When fetching users, isActive results set to false, should be ignored.
        // TODO: Salt the password before storing.
        // TODO: Fix the authenticated response to not contain dates.

        private static void ConfigureDependencies(WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IQueryExecutor, QueryExecutor>();
            builder.Services.AddTransient<ICommandExecutor, CommandExecutor>();
            builder.Services.AddTransient<IDbConnection, DbConnection>(sp => new MySqlConnection(GetConnectionString()));
            builder.Services.AddHttpContextAccessor();
            builder.Services.RegisterNtk8ExceptionHandlers();
            builder.Services.RegisterNtk8Services<User>();
            builder.Services.RegisterNkt8DatabaseServices<User>();
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