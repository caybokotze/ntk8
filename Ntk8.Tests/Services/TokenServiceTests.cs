using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NExpect;
using NSubstitute;
using Ntk8.DatabaseServices;
using Ntk8.Exceptions;
using Ntk8.Models;
using Ntk8.Services;
using Ntk8.Tests.TestHelpers;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Services
{
    [TestFixture]
    public class TokenServiceTests
    {
        [TestFixture]
        public class GenerateJwtToken
        {
            [Test]
            public void ShouldGenerateTokenThatIsNotNull()
            {
                // arrange
                var tokenService = Create();
                var user = TestUser.Create();
                var token = tokenService.GenerateJwtToken(user.Id, user.Roles);
                // act
                // assert
                Expect(token).Not.To.Be.Null();
            }

            [Test]
            [Repeat(5)]
            public void ShouldGenerateValidJwtToken()
            {
                // arrange
                var authSettings = CreateAuthSettings();
                var tokenService = Create(authSettings:authSettings);

                var user = TestUser.Create();

                // act
                var token = tokenService.GenerateJwtToken(user.Id, user.Roles);
                
                var header = token.Token
                    .Split(".")[0];

                var payload = token.Token
                    .Split(".")[1];
                
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token.Token);
                
                // assert
                Expect(header).To.Not.Be.Empty();
                Expect(payload).To.Not.Be.Empty();
                Expect(header).To.Be.Equal.To(jwtToken.Header.Base64UrlEncode());
                Expect(payload).To.Be.Equal.To(jwtToken.Payload.Base64UrlEncode());
                Expect(jwtToken.Claims.First(f => f.Type == "id").Value)
                    .To.Be.Equal.To(user.Id.ToString());
                Expect(jwtToken.IssuedAt).To.Approximately.Equal(DateTime.UtcNow);
                Expect(jwtToken.ValidFrom).To.Approximately.Equal(DateTime.UtcNow);
                Expect(jwtToken.ValidTo).To.Approximately.Equal(DateTime.UtcNow.AddSeconds(authSettings.JwtTTL));
                Expect(jwtToken.Issuer).To.Be.Equal.To("NTK8");
                Expect(jwtToken.SignatureAlgorithm).To.Equal("HS256");
                Expect(jwtToken.Claims.First(f => f.Type == "roles").Value)
                    .To.Equal(JsonConvert.SerializeObject(user.Roles));
            }

            [TestFixture]
            public class WhenAuthSettingSecretIsTooShort
            {
                [Test]
                public void ShouldThrow()
                {
                    // arrange
                    var authSettings = CreateAuthSettings();
                    authSettings.RefreshTokenSecret = GetRandomString(20, 22);
                    var tokenService = Create(authSettings:authSettings);
                    var user = TestUser.Create();
                    // act
                    // assert
                    Expect(() => tokenService
                            .GenerateJwtToken(user.Id, user.Roles))
                        .To
                        .Throw<InvalidTokenLengthException>();
                }
            }
        }

        [TestFixture]
        public class FetchUserAndCheckIfRefreshTokenIsActive
        {
            [Test]
            public void ShouldGetSingleRefreshTokenAndUser()
            {
                // arrange
                var ntk8Queries = Substitute.For<INtk8Queries<TestUser>>();
                var tokenService = Create(ntk8Queries);
                var user = TestUser.Create();
                
                var refreshToken = tokenService.GenerateRefreshToken(user.Id);

                user.RefreshToken = refreshToken;
                
                ntk8Queries.FetchUserByRefreshToken(Arg.Any<string>())
                    .Returns(user);

                var _ = tokenService
                    .IsRefreshTokenActive(refreshToken.Token ?? string.Empty);
                // act
                // assert
                Expect(ntk8Queries.FetchUserByRefreshToken(refreshToken.Token ?? string.Empty))
                    .To
                    .Equal(user);
            }
            
            [Test]
            public void ShouldThrowWhenNoTokensAreFound()
            {
                // arrange
                var ntk8Queries = Substitute.For<INtk8Queries<TestUser>>();
                var tokenService = Create(ntk8Queries);
                var user = TestUser.Create();
                var token = tokenService.GenerateJwtToken(user.Id, user.Roles!);
                var randomUser = TestUser.Create();
                randomUser.RefreshToken = null;

                ntk8Queries.FetchUserByRefreshToken(Arg.Any<string>())
                    .Returns(randomUser);
                
                // act
                // assert
                Expect(() => tokenService
                        .IsRefreshTokenActive(token.Token ?? string.Empty))
                    .To.Throw<InvalidRefreshTokenException>();
            }
        }
        
        private static TokenService<TestUser> Create(
            INtk8Queries<TestUser>? ntk8Queries = null,
            INtk8Commands? ntk8Commands = null,
            AuthSettings? authSettings = null,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            return new TokenService<TestUser>(
                ntk8Commands ?? Substitute.For<Ntk8Commands>(),
                ntk8Queries ?? Substitute.For<Ntk8Queries<TestUser>>(),
                authSettings ?? CreateAuthSettings(),
                httpContextAccessor ?? Substitute.For<IHttpContextAccessor>());
        }

        private static AuthSettings CreateAuthSettings()
        {
            return new AuthSettings
            {
                RefreshTokenSecret = GetRandomAlphaString(40),
                RefreshTokenTTL = 604800,
                JwtTTL = 1000
            };
        }
    }
}