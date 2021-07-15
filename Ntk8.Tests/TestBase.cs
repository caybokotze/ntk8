using System;
using System.Threading.Tasks;
using Dapper.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                    config.AddSingleton<IMemoryCache, MemoryCache>();
                    config.AddTransient<IQueryExecutor, QueryExecutor>();
                    config.AddTransient<ICommandExecutor, CommandExecutor>();
                    config.AddTransient<IAuthenticationContextService, AuthenticationContextService>();
                    config.AddTransient(_ => new AuthenticationConfiguration
                    {
                        RefreshTokenSecret = RandomValueGen.GetRandomAlphaString(),
                        RefreshTokenTTL = 3600
                    });
                });
            });

            var host = await hostBuilder.StartAsync();
            var serviceProvider = host.Services;
            
            ServiceProvider = serviceProvider;
        }
        
        public T Resolve<T>()
        {
            return (T) ServiceProvider.GetService(typeof(T));
        }
    }
}