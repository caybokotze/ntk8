using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper.CQRS;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NExpect;
using NSubstitute;
using Ntk8.Constants;
using Ntk8.Data.Queries;
using Ntk8.Middleware;
using Ntk8.Models;
using Ntk8.Services;
using Ntk8.Tests.ContextBuilders;
using Ntk8.Tests.Helpers;
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
            Expect(typeof(JwtMiddleware)
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
            public async Task ShouldAddAuthenticationRequestHeader()
            {
                // arrange
                var authSettings = GetRandom<AuthSettings>();
                var secret = GetRandomString(35);
                var user = GetRandom<BaseUser>();
                authSettings.RefreshTokenSecret = secret;
                var queryExecutor = Substitute.For<IQueryExecutor>();
                var tokenService = Substitute.For<ITokenService>();
                var validToken = TokenHelpers.CreateValidJwtToken(secret, user.Id);
                var validTokenAsString = TokenHelpers.CreateValidJwtTokenAsString(secret, user.Id);
                tokenService
                    .ValidateJwtSecurityToken(validTokenAsString, authSettings.RefreshTokenSecret)
                    .Returns(validToken);
                queryExecutor.Execute(Arg.Is<FetchUserById>(
                        u => u.Id == user.Id))
                    .Returns(user);
                var middleware = Substitute.For<JwtMiddleware>(authSettings, queryExecutor, tokenService);
                var httpContext = new HttpContextBuilder()
                    .WithRequest(new HttpRequestBuilder()
                        .WithHeaders(new HeaderDictionary
                        {
                            KeyValuePair
                                .Create<string, StringValues>(
                                    AuthenticationConstants.DefaultJwtHeader,
                                    validTokenAsString)
                        })
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
                Expect(middleware)
                    .To
                    .Have
                    .Received()
                    .MountUserToContext(httpContext, validTokenAsString);
            }
        }

        public class WhenMountingUserToContext
        {
            [Test]
            public void ShouldAttachAccountToHttpContext()
            {
                // arrange
                var queryExecutor = Substitute.For<IQueryExecutor>();
                var secret = GetRandomString(40);
                var user = GetRandom<BaseUser>();
                var token = TokenHelpers.CreateValidJwtToken(secret, user.Id);
                var tokenService = Substitute.For<ITokenService>();
                tokenService
                    .ValidateJwtSecurityToken(Arg.Any<string>(), Arg.Any<string>())
                    .Returns(token);

                var middleware = Create(new AuthSettings
                {
                    RefreshTokenSecret = secret
                },
                    queryExecutor,
                    tokenService);
                
                queryExecutor
                    .Execute(Arg.Is<FetchUserById>(f => f.Id == user.Id))
                    .Returns(user);
                var httpContext = new HttpContextBuilder()
                    .WithItems(new Dictionary<object, object>())
                    .Build();
                // act
                httpContext = middleware
                    .MountUserToContext(httpContext, TokenHelpers.CreateValidJwtTokenAsString(token));
                // assert
                Expect(httpContext.Items[AuthenticationConstants.ContextAccount])
                    .To
                    .Equal(user);
            }
        }

        public static JwtMiddleware Create(
            IAuthSettings authSettings = null, 
            IQueryExecutor queryExecutor = null,
            ITokenService tokenService = null)
        {
            return new JwtMiddleware(
                authSettings ?? new AuthSettings
                {
                    RefreshTokenSecret = GetRandomString(40),
                    RefreshTokenTTL = 3600,
                    JwtTTL = 1000
                },
                queryExecutor ?? Substitute.For<IQueryExecutor>(),
                tokenService ?? Substitute.For<ITokenService>());
        }
    }
}