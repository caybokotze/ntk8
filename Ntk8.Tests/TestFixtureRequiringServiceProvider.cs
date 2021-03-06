using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Transactions;
using Dapper.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using Ntk8.Infrastructure;
using Ntk8.Tests.TestHelpers;
using NUnit.Framework;

namespace Ntk8.Tests
{
    public class TestFixtureRequiringServiceProvider
    {
        protected IServiceProvider? ServiceProvider { get; set; }
        private HttpContext? HttpContext { get; set; }

        [SetUp]
        public async Task SetupHostEnvironment()
        {
            var appSettings = AppSettingProvider
                .CreateConfigurationRoot();
            
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.Configure(app =>
                {
                    app.Run(handle =>
                    {
                        HttpContext = handle;
                        return handle
                            .Response
                            .StartAsync();
                    });

                    app.Build();
                });

                webHost.ConfigureServices(config =>
                {
                    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                    config.AddSingleton<IMemoryCache, MemoryCache>();
                    config.AddTransient<IQueryExecutor, QueryExecutor>();
                    config.AddTransient<ICommandExecutor, CommandExecutor>();
                    config.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    config.AddTransient<IDbConnection, DbConnection>(_ => new MySqlConnection(appSettings.GetDefaultConnection()));
                    config.ConfigureNkt8Settings(appSettings);
                    config.RegisterNtk8Services<TestUser>();
                    config.RegisterNtk8ExceptionHandlers();
                    config.RegisterNkt8DatabaseServices<TestUser>();
                });
            });

            var host = await hostBuilder.StartAsync();
            var serviceProvider = host.Services;

            ServiceProvider = serviceProvider;
        }

        protected T Resolve<T>() where T : notnull
        {
            if (Transaction.Current is null)
            {
                throw new TransactionException("Tests that resolve real dependencies should be created inside of a transaction.");
            }
            
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}