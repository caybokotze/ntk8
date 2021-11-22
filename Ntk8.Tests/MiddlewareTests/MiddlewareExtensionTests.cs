using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NExpect;
using Ntk8.Exceptions;
using Ntk8.Helpers;
using Ntk8.Models;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using static NExpect.Expectations;

namespace Ntk8.Tests.MiddlewareTests
{
    [TestFixture]
    public class MiddlewareExtensionTests
    {
        [TestFixture]
        public class WhenResolvingForApplicationSettings
        {
            [TestFixture]
            public class WhenTokenLengthIsInvalid
            {
                [Test]
                public void ShouldThrowException()
                {
                    // arrange
                    var authenticationSettings = "{\"AuthSettings\" : {" +
                    "\"RefreshTokenSecret\": \"abcd\"," +
                    "\"RefreshTokenTTL\": 3600," +
                    "\"JwtTTL\": 1," +
                    "\"PasswordResetTokenTTL\": 500," +
                    "\"UserVerificationTokenTTL\": 500 }}";
                    var randomFileName = RandomValueGen.GetRandomString();
                    File.WriteAllText(Path.Combine(
                        Directory.GetCurrentDirectory(), $"{randomFileName}.json"), authenticationSettings);
                    // act
                    var configurationBuilder = new ConfigurationBuilder();
                    configurationBuilder.AddJsonFile($"{randomFileName}.json");
                    var configurationRoot = configurationBuilder.Build();
                    
                    var hostBuilder = new HostBuilder();
                    hostBuilder.ConfigureWebHost(host =>
                    {
                        host.UseTestServer();
                        host.ConfigureServices(config =>
                        {
                            config.RegisterAndConfigureNtk8AuthenticationSettings(configurationRoot);
                        });
                    });
                    // assert
                    Expect(() => hostBuilder.StartAsync())
                        .To
                        .Throw<InvalidTokenLengthException>()
                        .With.Message.Containing("The token secret must be at least 32 characters long");
                }
            }

            [Test]
            public async Task ShouldResolveForAppSettingsAsSingleton()
            {
                // arrange
                var appSettings = AppSettingProvider
                    .CreateConfigurationRoot();
                var hostBuilder = new HostBuilder();
                hostBuilder.ConfigureWebHost(host =>
                {
                    host.UseTestServer();
                    host.Configure(app =>
                    {
                        app.Run(handle => handle
                            .Response
                            .StartAsync());
                    });
                    
                    host.ConfigureServices(config =>
                    {
                        config.RegisterAndConfigureNtk8AuthenticationSettings(appSettings);
                    });
                });
                var host = await hostBuilder.StartAsync();
                var serviceProvider = host.Services;
                // act
                var firstResolve = serviceProvider.GetRequiredService<IAuthSettings>();
                var secondResolve = serviceProvider.GetRequiredService<IAuthSettings>();
                // assert
                Expect(firstResolve).To.Equal(secondResolve);
                Expect(firstResolve.GetHashCode()).To.Equal(secondResolve.GetHashCode());
            }
        }
    }
}