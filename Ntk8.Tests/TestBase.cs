using System;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using Dapper.CQRS;
using Dapper.CQRS.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using Ntk8.Models;
using Ntk8.Services;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

namespace Ntk8.Tests
{
    [TestFixture]
    public class TestBase
    {
        public IServiceProvider ServiceProvider { get; set; }
        
        [SetUp]
        public async Task SetupHostEnvironment()
        {
            var appSettings = AppSettingProvider.CreateConfig();
            
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
                    config.AddTransient<AuthenticationContextService>();
                    config.AddTransient(_ => new AuthSettings
                    {
                        RefreshTokenSecret = RandomValueGen.GetRandomAlphaString(),
                        RefreshTokenTTL = 3600
                    });
                    config.AddTransient<IDbConnection>(p =>
                        new MySqlConnection(AppSettingProvider.CreateAppSettings().DefaultConnection));
                    config.AddTransient<IBaseSqlExecutorOptions>(provider => new BaseSqlExecutorOptions
                    {
                        Connection = Resolve<IDbConnection>(),
                        Dbms = DBMS.MySQL,
                        ServiceProvider = provider
                    });
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
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}