using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NExpect;
using Ntk8.Exceptions;
using Ntk8.Helpers;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

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
                    File.WriteAllText(Path
                        .Combine(Directory
                            .GetCurrentDirectory(), $"{randomFileName}.json"), authenticationSettings);
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
                    Expectations.Expect(() => hostBuilder.StartAsync())
                        .To
                        .Throw<InvalidTokenLengthException>()
                        .With.Message.Containing("The token secret must be at least 32 characters long");
                }
            }
            
            [Test]
            public void ShouldResolveForAuthenticationSettings()
            {
                // arrange
                
                // act
                // assert
            }
        }
    }
}