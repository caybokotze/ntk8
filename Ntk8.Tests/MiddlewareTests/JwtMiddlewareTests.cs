using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dapper.CQRS;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using NExpect;
using NSubstitute;
using Ntk8.Constants;
using Ntk8.Data.Queries;
using Ntk8.Middleware;
using Ntk8.Models;
using Ntk8.Tests.ContextBuilders;
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
                var middleware = Substitute.For<JwtMiddleware>(
                    GetRandom<AuthSettings>(),
                    GetRandom<IQueryExecutor>());
                
                var httpContext = new HttpContextBuilder()
                    .Build();
                
                var requestDelegate = Substitute.For<RequestDelegate>();
                // act
                // assert
                Expect(() => middleware
                        .InvokeAsync(httpContext, requestDelegate))
                    .To
                    .Throw<NullReferenceException>()
                    .With.Message.Containing("Object reference not set");
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
                queryExecutor.Execute(Arg.Is<FetchUserById>(
                        u => u.Id == user.Id))
                    .Returns(user);
                var middleware = Substitute.For<JwtMiddleware>(
                    authSettings,
                    GetRandom<IQueryExecutor>());
                var validToken = CreateValidJwtToken(secret, user.Id);
                var httpContext = new HttpContextBuilder()
                    .WithRequest(new HttpRequestBuilder()
                        .WithHeaders(new HeaderDictionary
                        {
                            KeyValuePair
                                .Create<string, StringValues>(
                                    AuthenticationConstants.DefaultJwtHeader,
                                    validToken)
                        }).Build())
                    .WithItems(new Dictionary<object, object>())
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
                    .MountUserToContext(httpContext, validToken);
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
                var middleware = Create(new AuthSettings
                {
                    RefreshTokenSecret = secret
                },
                    queryExecutor);
                var user = GetRandom<BaseUser>();
                queryExecutor
                    .Execute(Arg.Is<FetchUserById>(f => f.Id == user.Id))
                    .Returns(user);
                var httpContext = new HttpContextBuilder()
                    .WithItems(new Dictionary<object, object>())
                    .Build();
                var token = CreateValidJwtToken(secret, user.Id);
                // act
                httpContext = middleware
                    .MountUserToContext(httpContext, token);
                // assert
                Expect(httpContext.Items[AuthenticationConstants.ContextAccount])
                    .To
                    .Equal(user);
            }
        }

        public static string CreateValidJwtToken(string secret = null, int? userId = null)
        {
            userId ??= GetRandomInt();
            secret ??= GetRandomString(50);

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(
                new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(AuthenticationConstants.PrimaryKeyValue, userId.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                        SecurityAlgorithms.HmacSha256Signature)
                });
            return tokenHandler.WriteToken(token);
        }

        public static JwtMiddleware Create(
            AuthSettings authSettings = null, 
            IQueryExecutor queryExecutor = null)
        {
            authSettings ??= new AuthSettings()
            {
                RefreshTokenSecret = GetRandomString(40),
                RefreshTokenTTL = 3600
            };
            
            return new JwtMiddleware(
                authSettings,
                queryExecutor ?? Substitute.For<IQueryExecutor>());
        }
    }
}