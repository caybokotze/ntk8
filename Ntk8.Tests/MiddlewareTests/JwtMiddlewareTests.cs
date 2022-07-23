using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper.CQRS;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NExpect;
using NSubstitute;
using Ntk8.Constants;
using Ntk8.Data.Queries;
using Ntk8.DatabaseServices;
using Ntk8.Middleware;
using Ntk8.Models;
using Ntk8.Services;
using Ntk8.Tests.ContextBuilders;
using Ntk8.Tests.TestHelpers;
using Ntk8.Tests.TestModels;
using Ntk8.Utilities;
using NUnit.Framework;
using PeanutButter.Utils;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.MiddlewareTests
{
    [TestFixture]
    public class JwtMiddlewareTests
    {
        [Test]
        public void ShouldBeMiddleware()
        {
            // arrange
            // act
            // assert
            Expect(typeof(JwtMiddleware<TestUser>)
                .Implements(typeof(IMiddleware)))
                .To
                .Be
                .Equal
                .To(true);
        }

        [TestFixture]
        public class WhenMiddlewareIsInvoked
        {
            [Test]
            public void ShouldThrowForNullAuthenticationToken()
            {
                // arrange
                var middleware = Create();

                var httpContext = new HttpContextBuilder()
                    .Build();

                var requestDelegate = Substitute
                    .For<RequestDelegate>();
                // act
                // assert
                Expect(() => middleware
                        .InvokeAsync(httpContext, requestDelegate))
                    .To
                    .Throw<NullReferenceException>()
                    .With
                    .Message
                    .Containing("Object reference not set");
            }


            [Test]
            public async Task ShouldAddUserToTheHttpContext()
            {
                // arrange
                var authSettings = TestUser.CreateValidAuthenticationSettings();
                var user = TestUser.Create();
                var queries = Substitute.For<INtk8Queries<TestUser>>();
                var tokenService = Substitute.For<ITokenService>();
                var validToken = TestTokenHelpers.CreateValidJwtToken(authSettings.RefreshTokenSecret, user.Id);
                var validTokenAsString = TestTokenHelpers.CreateValidJwtTokenAsString(authSettings.RefreshTokenSecret!, user.Id);
                
                tokenService
                    .ValidateJwtSecurityToken(validTokenAsString, authSettings.RefreshTokenSecret!)
                    .Returns(validToken);
                
                queries.FetchUserById(user.Id).Returns(user);

                var globalSettings = new GlobalSettings
                {
                    UseJwt = true
                };

                var middleware = Substitute
                    .For<JwtMiddleware<TestUser>>(
                        queries, 
                        authSettings, 
                        tokenService,
                        globalSettings);

                var httpContext = new HttpContextBuilder()
                    .WithRequest(new HttpRequestBuilder()
                        .WithHeaders(new HeaderDictionary
                        {
                            KeyValuePair
                                .Create<string, StringValues>(
                                    AuthenticationConstants.DefaultJwtHeader,
                                    validTokenAsString)
                        })
                        .WithCookies(new MockRequestCookieCollection(new Dictionary<string, string?>
                        {
                            // { AuthenticationConstants.RefreshToken, GetRandomString() }
                        }))
                        .Build())
                    .WithItems(new Dictionary<object, object>())
                    .WithResponse(new HttpResponseBuilder()
                        .HasStarted(false)
                        .Build())
                    .Build();
                
                var requestDelegate = Substitute.For<RequestDelegate>();
                
                // act
                
                await middleware
                    .InvokeAsync(httpContext, requestDelegate);
                
                // assert

                Expect(httpContext.GetCurrentUser()).To.Equal(user);
            }
        }

        public class WhenMountingUserToContext
        {
            [Test]
            public void ShouldAttachAccountToHttpContext()
            {
                // arrange
                var ntk8Query = Substitute.For<INtk8Queries<TestUser>>();
                var secret = GetRandomString(40);
                var user = TestUser.Create();
                var token = TestTokenHelpers.CreateValidJwtToken(secret, user.Id);
                var tokenService = Substitute.For<ITokenService>();
                tokenService
                    .ValidateJwtSecurityToken(Arg.Any<string>(), Arg.Any<string>())
                    .Returns(token);

                var middleware = Create(new AuthSettings
                {
                    RefreshTokenSecret = secret
                },
                    ntk8Query,
                    tokenService);

                ntk8Query.FetchUserById(user.Id).Returns(user);
              
                var httpContext = new HttpContextBuilder()
                    .WithItems(new Dictionary<object, object>())
                    .Build();
                // act
                middleware
                    .MountUserToContext(httpContext, TestTokenHelpers.CreateValidJwtTokenAsString(token), true);
                // assert
                Expect(httpContext.Items[AuthenticationConstants.CurrentUser])
                    .To
                    .Equal(user);
            }
        }

        public static JwtMiddleware<TestUser> Create(
            IAuthSettings? authSettings = null, 
            INtk8Queries<TestUser>? ntk8Queries = null,
            ITokenService? tokenService = null)
        {
            return new JwtMiddleware<TestUser>(
                ntk8Queries ?? Substitute.For<INtk8Queries<TestUser>>(),
                authSettings ?? new AuthSettings
                {
                    RefreshTokenSecret = GetRandomString(40),
                    RefreshTokenTTL = 3600,
                    JwtTTL = 1000
                },
                tokenService ?? Substitute.For<ITokenService>(),
                GetRandom<GlobalSettings>());
        }
    }
}