using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Dapper.CQRS;
using Newtonsoft.Json;
using NExpect;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Ntk8.Data.Queries;
using Ntk8.Exceptions;
using Ntk8.Helpers;
using Ntk8.Models;
using Ntk8.Services;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Helpers
{
    [TestFixture]
    public class AccountServiceHelperTests
    {
        [TestFixture]
        public class GenerateJwtToken
        {
            [Test]
            public void ShouldGenerateTokenThatIsNotNull()
            {
                // arrange
                var authSettings = new AuthSettings
                {
                    RefreshTokenSecret = GetRandomAlphaString(34)
                };
                
                var token = CreateJwtToken(authSettings);
                // act
                // assert
                Expect(token)
                    .Not
                    .To
                    .Be
                    .Null();
            }

            [Test]
            [Repeat(5)]
            public void ShouldGenerateValidJwtToken()
            {
                // arrange
                var authSettings = new AuthSettings
                {
                    RefreshTokenSecret = GetRandomString(35)
                };

                var user = TestUser.Create();
                user.Roles = CreateRandomRoles();

                // act
                var token = CreateJwtToken(authSettings, user);
                
                var header = token
                    .Split(".")[0];

                var payload = token
                    .Split(".")[1];
                
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                
                // assert
                Expect(header).To.Not.Be.Empty();
                Expect(payload).To.Not.Be.Empty();
                Expect(header).To.Be.Equal.To(jwtToken.Header.Base64UrlEncode());
                Expect(payload).To.Be.Equal.To(jwtToken.Payload.Base64UrlEncode());
                Expect(jwtToken.Claims.First(f => f.Type == "id").Value)
                    .To.Be.Equal.To(user.Id.ToString());
                Expect(jwtToken.IssuedAt).To.Approximately.Equal(DateTime.UtcNow);
                Expect(jwtToken.ValidFrom).To.Approximately.Equal(DateTime.UtcNow);
                Expect(jwtToken.ValidTo).To.Approximately.Equal(DateTime.UtcNow.AddMinutes(15));
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
                    var authSettings = new AuthSettings()
                    {
                        RefreshTokenSecret = GetRandomString(8, 29)
                    };
                    // act
                    // assert
                    Expect(() => CreateJwtToken(authSettings))
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
                var queryExecutor = Substitute.For<IQueryExecutor>();
                var randomUser = GetRandom<BaseUser>();
                var refreshToken = CreateRefreshToken();
                
                randomUser.RefreshTokens = new List<RefreshToken>()
                {
                    refreshToken
                };
                
                queryExecutor
                    .Execute(Arg.Any<FetchUserByRefreshToken>())
                    .Returns(randomUser);
                
                var baseUser = TokenService
                    .FetchUserAndCheckIfRefreshTokenIsActive(queryExecutor, refreshToken.Token);
                // act
                // assert
                Expect(queryExecutor.Execute(new FetchUserByRefreshToken(refreshToken.Token)))
                    .To
                    .Equal(baseUser);
            }
            
            [Test]
            public void ShouldThrowWhenNoTokensAreFound()
            {
                // arrange
                var queryExecutor = Substitute.For<IQueryExecutor>();
                var token = CreateJwtToken();
                var randomUser = GetRandom<BaseUser>();
                queryExecutor
                    .Execute(Arg.Any<FetchUserByRefreshToken>())
                    .Returns(randomUser);
                // act
                // assert
                Expect(() => TokenService
                        .FetchUserAndCheckIfRefreshTokenIsActive(queryExecutor, token))
                    .To.Throw<InvalidOperationException>();
            }
        }

        [TestFixture]
        public class RemoveOldRefreshTokens
        {
            [Test]
            public void ShouldRemoveOldRefreshTokens()
            {
                // arrange
                var user = TestUser.Create();
                var authSettings = CreateAuthSettings();
                var oldRefreshToken = CreateRefreshToken();
                var newRefreshToken = CreateRefreshToken();
                oldRefreshToken.Expires = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
                oldRefreshToken.DateRevoked = DateTime.UtcNow;
                oldRefreshToken.DateCreated = DateTime.UtcNow.Subtract(TimeSpan.FromDays(8));
                
                user.RefreshTokens = new List<RefreshToken>
                {
                    newRefreshToken,
                    oldRefreshToken
                };
                // act
                var expectedUser = TokenService
                    .RemoveOldRefreshTokens(authSettings, user);
                // assert
                Expect(expectedUser.RefreshTokens.Count).To.Equal(1);
                Expect(expectedUser.RefreshTokens).To.Contain(newRefreshToken);
            }

            [Test]
            public void ShouldNotRemoveValidRefreshTokens()
            {
                // arrange
                var user = TestUser.Create();
                var authSettings = CreateAuthSettings();
                var newRefreshToken = CreateRefreshToken();

                user.RefreshTokens = new List<RefreshToken>
                {
                    newRefreshToken
                };
                // act
                var expectedUser = TokenService
                    .RemoveOldRefreshTokens(authSettings, user);
                // assert
                Expect(expectedUser.RefreshTokens.Count).To.Equal(1);
                Expect(expectedUser.RefreshTokens).To.Contain(newRefreshToken);
            }
        }

        [TestFixture]
        public class GetAccount
        {
            [TestFixture]
            public class WhenUserCanNotBeFound
            {
                [Test]
                public void ShouldThrow()
                {
                    // arrange
                    var queryExecutor = Substitute.For<IQueryExecutor>();
                    var user = TestUser.Create();
                    queryExecutor
                        .Execute(Arg.Is<FetchUserById>(f => f.Id == user.Id))
                        .ReturnsNull();
                    // act
                    // assert
                    Expect(() => TokenService.GetAccount(queryExecutor, user.Id))
                        .To.Throw<UserNotFoundException>()
                        .With.Message.Containing("The user can not be found");
                }
            }

            [TestFixture]
            public class WhenUserCanBeFound
            {
                [Test]
                public void ShouldReturnUser()
                {
                    // arrange
                    var queryExecutor = Substitute.For<IQueryExecutor>();
                    var user = TestUser.Create();
                    queryExecutor
                        .Execute(Arg.Is<FetchUserById>(f => f.Id == user.Id))
                        .Returns(user);
                    // act
                    var expectedUser = TokenService.GetAccount(queryExecutor, user.Id);
                    // assert
                    Expect(expectedUser).To.Equal(user);
                }
            }
        }

        private static string CreateJwtToken(
            AuthSettings authSettings = null, 
            BaseUser baseUser = null)
        {
            return TokenService.GenerateJwtToken(
                authSettings ?? new AuthSettings
                {
                    RefreshTokenSecret = GetRandomString(35)
                },
                baseUser ?? GetRandom<BaseUser>());
        }

        private static AuthSettings CreateAuthSettings()
        {
            return new()
            {
                RefreshTokenSecret = GetRandomAlphaString(40),
                RefreshTokenTTL = 604800
            };
        }

        private static Role[] CreateRandomRoles()
        {
            return new Role[]
            {
                new()
                {
                    RoleName = GetRandomString()
                },
                new()
                {
                    RoleName = GetRandomString()
                }
            };
        }

        private static RefreshToken CreateRefreshToken()
        {
            return TokenService.GenerateRefreshToken(
                CreateAuthSettings(),
                GetRandomIPv4Address()
            );
        }
    }
}