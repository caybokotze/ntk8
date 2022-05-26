using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NExpect;
using Ntk8.Infrastructure;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.HelperTests;

[TestFixture]
public class Ntk8CustomSqlStatementTests
{
    [TestFixture]
    public class WhenMakingUseOfCustomSqlScripts
    {
        [Test]
        public async Task ShouldResolveStatements()
        {
            // arrange
            var randomSql = GetRandomString();

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
                        config.ConfigureNtk8CustomSql(s =>
                        {
                            s.FetchUserByEmailAddressStatement = randomSql;
                            s.FetchUserByIdStatement = randomSql;
                            s.InsertUserStatement = randomSql;
                            s.FetchUserByRefreshTokenStatement = randomSql;
                            s.FetchUserByResetTokenStatement = randomSql;
                            s.FetchUserByVerificationTokenStatement = randomSql;
                        });
                    });
                });

            var host = await hostBuilder.StartAsync();
            var serviceProvider = host.Services;
            
            // act
            var customSql = serviceProvider.GetRequiredService<Ntk8CustomSqlStatements>();

            // assert
            Expect(customSql.FetchUserByEmailAddressStatement)
                .To.Equal(randomSql);
            Expect(customSql.FetchUserByIdStatement)
                .To.Equal(randomSql);
            Expect(customSql.InsertUserStatement)
                .To.Equal(randomSql);
            Expect(customSql.FetchUserByRefreshTokenStatement)
                .To.Equal(randomSql);
            Expect(customSql.FetchUserByResetTokenStatement)
                .To.Equal(randomSql);
            Expect(customSql.FetchUserByVerificationTokenStatement)
                .To.Equal(randomSql);
        }
    }
}