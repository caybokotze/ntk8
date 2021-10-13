using System.Data;
using System.Data.Common;
using Dapper.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
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
            builder.Services.AddTransient<IQueryExecutor, QueryExecutor>();
            builder.Services.AddTransient<ICommandExecutor, CommandExecutor>();
            builder.Services.AddTransient<IDbConnection, DbConnection>(sp => new MySqlConnection());
            builder.Services.AddTransient<HttpContextAccessor>();
            builder.Services.AddTransient(sp => ResolveAuthSettings());
            
            var app = builder.Build();
            var _ = new AuthHandler(app, 
                app.Resolve<IAccountService>(),
                app.Resolve<AuthenticationContextService>());
            app.Run();
        }

        public static AuthSettings ResolveAuthSettings()
        {
            return JsonConvert.DeserializeObject<AuthSettings>(CreateConfigurationRoot()
                .GetSection("AuthSettings")
                .Value);
        }
    }
}