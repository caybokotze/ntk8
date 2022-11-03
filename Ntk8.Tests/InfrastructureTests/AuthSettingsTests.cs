using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using NExpect;
using Ntk8.Infrastructure;
using Ntk8.Models;
using Ntk8.Tests.TestHelpers;
using NUnit.Framework;
using static NExpect.Expectations;

namespace Ntk8.Tests.InfrastructureTests;

public class AuthSettingsTests
{
    [Test]
    public void ShouldContainExpectedDefaultValues()
    {
        // arrange
        var authSettings = new AuthSettings();
        // act
        // assert
        Expect(authSettings.RefreshTokenSecret).To.Not.Be.Empty();
        Expect(authSettings.RefreshTokenLength).To.Equal(40);
        Expect(authSettings.JwtTTL).To.Equal(900);
        Expect(authSettings.RefreshTokenTTL).To.Equal(3600);
        Expect(authSettings.PasswordResetTokenTTL).To.Equal(900);
        Expect(authSettings.UserVerificationTokenTTL).To.Equal(900);
    }

    [TestFixture]
    public class Resolution
    {
        [Test]
        public async Task DoesResolveAuthSettingsAsExpected()
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
                        config.ConfigureNtk8<TestUserEntity>(options =>
                        {
                            options.ConfigureAuthSettings(c =>
                            {
                                c.RefreshTokenLength = 32;
                                c.JwtTTL = 945;
                                c.PasswordResetTokenTTL = 1050;
                                c.RefreshTokenSecret = "abcd";
                                c.RefreshTokenTTL = 2300;
                                c.UserVerificationTokenTTL = 400;
                            });
                        });
                    });
                });

            var host = await hostBuilder.StartAsync();
            var serviceProvider = host.Services;

            // act
            var retrievedAuthSettings = serviceProvider.GetRequiredService<IAuthSettings>();
            
            // assert
            Expect(retrievedAuthSettings.RefreshTokenLength).To.Equal(32);
            Expect(retrievedAuthSettings.JwtTTL).To.Equal(945);
            Expect(retrievedAuthSettings.PasswordResetTokenTTL).To.Equal(1050);
            Expect(retrievedAuthSettings.RefreshTokenSecret).To.Equal("abcd");
            Expect(retrievedAuthSettings.RefreshTokenTTL).To.Equal(2300);
            Expect(retrievedAuthSettings.UserVerificationTokenTTL).To.Equal(400);
        }
    }
}