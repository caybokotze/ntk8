using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using NExpect;
using Ntk8.DatabaseServices;
using Ntk8.Infrastructure;
using Ntk8.Tests.TestHelpers;
using Ntk8.Tests.TestModels;
using NUnit.Framework;
using static NExpect.Expectations;

namespace Ntk8.Tests.InfrastructureTests;

public class ContainerExtensionTests
{
    [TestFixture]
    public class WhenResolvingDependencies
    {
        [TestFixture]
        public class WhenResolvingTransient
        {
            
        }

        [TestFixture]
        public class WhenResolvingSingleton
        {
            
        }
    }
    
    [TestFixture]
    public class WhenConfiguringCustomOptions
    {
        [TestFixture]
        public class WhenConfiguringCustomCommandType
        {
            [Test]
            public async Task ShouldUseSpecifiedType()
            {
                // arrange
                var hostBuilder = new HostBuilder()
                    .ConfigureWebHost(webHost =>
                    {
                        webHost.UseTestServer();
                        webHost.Configure(app =>
                        {
                            app.Run(handle => handle
                                .Response
                                .StartAsync());

                            app.Build();
                        });

                        webHost.ConfigureServices(config =>
                        {
                            config.ConfigureNtk8<TestUser>(c => { c.OverrideUserCommands<TestCommandType>(); });
                        });
                    });

                var host = await hostBuilder.StartAsync();
                var serviceProvider = host.Services;
                // act
                var resolvedType = serviceProvider.GetRequiredService<IAccountCommands>();
                // assert
                Expect(resolvedType.GetType()).To.Equal(typeof(TestCommandType));
            }

            [TestFixture]
            public class WhenNotSet
            {
                [Test]
                public async Task ShouldUseDefaultRegisteredType()
                {
                    // arrange
                    var hostBuilder = new HostBuilder()
                        .ConfigureWebHost(webHost =>
                        {
                            webHost.UseTestServer();
                            webHost.Configure(app =>
                            {
                                app.Run(handle => handle
                                    .Response
                                    .StartAsync());

                                app.Build();
                            });

                            webHost.ConfigureServices(config =>
                            {
                                config.ConfigureNtk8<TestUser>();
                                config.AddTransient<IQueryExecutor, QueryExecutor>();
                                config.AddTransient<ICommandExecutor, CommandExecutor>();
                                config.AddTransient<IExecutable, Executable>();
                                config.AddTransient<IQueryable, Queryable>();
                                config.AddTransient<IDbConnection, DbConnection>(_ => new MySqlConnection());
                            });
                        });

                    var host = await hostBuilder.StartAsync();
                    var serviceProvider = host.Services;
                    // act
                    var resolvedType = serviceProvider.GetRequiredService<IAccountCommands>();
                    // assert
                    Expect(resolvedType.GetType())
                        .To
                        .Equal(typeof(AccountCommands));
                }
            }
        }

        [TestFixture]
        public class WhenConfiguringCustomQueryType
        {
            [Test]
            public async Task ShouldUseSpecifiedType()
            {
                // arrange
                var hostBuilder = new HostBuilder()
                    .ConfigureWebHost(webHost =>
                    {
                        webHost.UseTestServer();
                        webHost.Configure(app =>
                        {
                            app.Run(handle => handle
                                .Response
                                .StartAsync());

                            app.Build();
                        });

                        webHost.ConfigureServices(config =>
                        {
                            config.ConfigureNtk8<TestUser>(c => { c.OverrideUserQueries<TestQueryType>(); });
                        });
                    });

                var host = await hostBuilder.StartAsync();
                var serviceProvider = host.Services;
                // act
                var resolvedType = serviceProvider.GetRequiredService<IAccountQueries>();
                // assert
                Expect(resolvedType.GetType()).To.Equal(typeof(TestQueryType));
            }

            [TestFixture]
            public class WhenNotSet
            {
                [Test]
                public async Task ShouldUseDefaultRegisteredType()
                {
                    // arrange
                    var hostBuilder = new HostBuilder()
                        .ConfigureWebHost(webHost =>
                        {
                            webHost.UseTestServer();
                            webHost.Configure(app =>
                            {
                                app.Run(handle => handle
                                    .Response
                                    .StartAsync());

                                app.Build();
                            });

                            webHost.ConfigureServices(config =>
                            {
                                config.ConfigureNtk8<TestUser>();
                                config.AddTransient<IQueryExecutor, QueryExecutor>();
                                config.AddTransient<IExecutable, Executable>();
                                config.AddTransient<IQueryable, Queryable>();
                                config.AddTransient<IDbConnection, DbConnection>(_ => new MySqlConnection());
                            });
                        });

                    var host = await hostBuilder.StartAsync();
                    var serviceProvider = host.Services;
                    // act
                    var resolvedType = serviceProvider.GetRequiredService<IAccountQueries>();
                    // assert
                    Expect(resolvedType.GetType())
                        .To
                        .Equal(typeof(AccountQueries));
                }
            }
        }
    }
}