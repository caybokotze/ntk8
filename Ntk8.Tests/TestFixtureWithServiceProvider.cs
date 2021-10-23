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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using Ntk8.Models;
using Ntk8.Services;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

namespace Ntk8.Tests
{
    [TestFixture]
    public class TestFixtureWithServiceProvider
    {
        public IServiceProvider ServiceProvider { get; set; }
        
        [SetUp]
        public async Task SetupHostEnvironment()
        {
            var appSettings = AppSettingProvider.CreateConfigurationRoot();
            
            var hostBuilder = new HostBuilder().ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.Configure(app =>
                {
                    app.Run(handle => handle
                        .Response
                        .StartAsync());
                });

                webHost.ConfigureServices(config =>
                {
                    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                    config.AddSingleton<IMemoryCache, MemoryCache>();
                    config.AddTransient<IQueryExecutor, QueryExecutor>();
                    config.AddTransient<ICommandExecutor, CommandExecutor>();
                    config.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
                    config.AddTransient<IAuthenticationContextService, AuthenticationContextService>();
                    config.AddTransient<ITokenService, TokenService>();
                    config.AddTransient<IAuthSettings, AuthSettings>();
                    config.AddTransient(
                        _ => new AuthSettings
                    {
                        RefreshTokenSecret = RandomValueGen.GetRandomAlphaString(),
                        RefreshTokenTTL = 3600,
                        JwtTTL = 1000
                    });
                    config.AddTransient<IDbConnection, DbConnection>(p =>
                        new MySqlConnection(AppSettingProvider.CreateAppSettings().DefaultConnection));
                    config.AddTransient<IQueryExecutor, QueryExecutor>();
                    config.AddTransient<ICommandExecutor, CommandExecutor>();
                    config.AddTransient<IAccountService, AccountService>();
                    config.Configure<IAuthSettings>(options => appSettings.GetSection("AuthSettings").Bind(options));
                });
            });

            var host = await hostBuilder.StartAsync();
            var serviceProvider = host.Services;
            
            ServiceProvider = serviceProvider;
        }
        
        public T Resolve<T>()
        {
            if (Transaction.Current is null)
            {
                throw new TransactionException("Tests that resolve real dependencies should be created inside of a transaction.");
            }
            
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}