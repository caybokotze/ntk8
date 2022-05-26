using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Dapper.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NExpect;
using Ntk8.Exceptions;
using Ntk8.Exceptions.Middleware;
using Ntk8.Infrastructure;
using Ntk8.Middleware;
using Ntk8.Models;
using Ntk8.Services;
using Ntk8.Tests.Helpers;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using static NExpect.Expectations;

namespace Ntk8.Tests.Infrastructure;

public class DependencyResolutionTests
{
     [TestFixture]
        public class ScopeResolutionTests
        {
            [TestFixture]
            public class Dependencies : TestFixtureRequiringServiceProvider
            {
                [TestCase(typeof(IHttpContextAccessor))]
                [TestCase(typeof(IAuthSettings))]
                [TestCase(typeof(IBaseUser))]
                public void ShouldResolveAsSingleton(Type type)
                {
                    // arrange
                    // act
                    var firstResolve = ServiceProvider?.GetService(type);
                    var secondResolve = ServiceProvider?.GetService(type);
                    // assert
                    Expect(firstResolve).To.Not.Be.Null();
                    Expect(secondResolve).To.Not.Be.Null();
                    Expect(firstResolve).To.Equal(secondResolve);
                    Expect(firstResolve?.GetHashCode()).To.Equal(secondResolve?.GetHashCode());
                }
            
                [TestCase(typeof(IQueryExecutor))]
                [TestCase(typeof(ICommandExecutor))]
                [TestCase(typeof(IDbConnection))]
                [TestCase(typeof(IAccountService))]
                [TestCase(typeof(ITokenService))]
                public void ShouldResolveAsTransient(Type type)
                {
                    // arrange
                    // act
                    var firstResolve = ServiceProvider?.GetService(type);
                    var secondResolve = ServiceProvider?.GetService(type);
                    // assert
                    Expect(firstResolve).To.Not.Be.Null();
                    Expect(secondResolve).To.Not.Be.Null();
                    Expect(firstResolve).Not.To.Equal(secondResolve);
                    Expect(firstResolve?.GetHashCode()).Not.To.Equal(secondResolve?.GetHashCode());
                }
            }

            [TestFixture]
            public class Middleware : TestFixtureRequiringServiceProvider
            {
                [TestCase(typeof(UserNotAuthorisedExceptionMiddleware))]
                [TestCase(typeof(UserNotAuthenticatedExceptionMiddleware))]
                [TestCase(typeof(UserNotFoundExceptionMiddleware))]
                [TestCase(typeof(UserAlreadyExistsExceptionMiddleware))]
                [TestCase(typeof(UserIsVerifiedExceptionMiddleware))]
                [TestCase(typeof(UserIsVerifiedExceptionMiddleware))]
                [TestCase(typeof(VerificationTokenExpiredExceptionMiddleware))]
                public void ShouldResolveAsSingleton(Type type)
                {
                    // arrange
                    // act
                    var firstResolve = ServiceProvider?.GetService(type);
                    var secondResolve = ServiceProvider?.GetService(type);
                    // assert
                    Expect(firstResolve).To.Not.Be.Null();
                    Expect(secondResolve).To.Not.Be.Null();
                    Expect(firstResolve).To.Equal(secondResolve);
                    Expect(firstResolve?.GetHashCode()).To.Equal(secondResolve?.GetHashCode());
                }
            }
        }
        
        [TestFixture]
        public class WhenResolvingForApplicationSettings
        {
            [TestFixture]
            public class AndTokenLengthIsInvalid
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
                        host.Configure(app =>
                        {
                            app.Run(handle => handle
                                .Response
                                .StartAsync());
                        });
                        host.ConfigureServices(config =>
                        {
                            config.ConfigureNkt8Settings(configurationRoot);
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
                        config.ConfigureNkt8Settings(appSettings);
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

        [TestFixture]
        public class WhenResolvingForJwtMiddleware : TestFixtureRequiringServiceProvider
        {
            [Test]
            public void ShouldResolveAsTransient()
            {
                // arrange
                // act
                var firstResolve = ServiceProvider?.GetRequiredService<JwtMiddleware<TestUser>>();
                var secondResolve = ServiceProvider?.GetRequiredService<JwtMiddleware<TestUser>>();
                // assert
                Expect(firstResolve).To.Not.Be.Null();
                Expect(secondResolve).To.Not.Be.Null();
                Expect(firstResolve).Not.To.Equal(secondResolve);
            }
        }
}