using System;
using System.Transactions;
using Dapper.CQRS;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NExpect;
using NSubstitute;
using Ntk8.DatabaseServices;
using Ntk8.Dto;
using Ntk8.Exceptions;
using Ntk8.Models;
using Ntk8.Services;
using Ntk8.Tests.TestHelpers;
using Ntk8.Utilities;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Services
{
    [TestFixture]
    public class AccountServiceTests
    {
        [TestFixture]
        public class DependencyInjection : TestFixtureRequiringServiceProvider
        {
            [Test]
            public void ShouldNotBeNull()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var accountService = Resolve<IAccountService>();
                    var commandExecutor = Resolve<ICommandExecutor>();
                    var queryExecutor = Resolve<IQueryExecutor>();
                    var tokenService = Resolve<ITokenService>();
                    // act
                    // assert
                    Expect(accountService).Not.To.Be.Null();
                    Expect(commandExecutor).Not.To.Be.Null();
                    Expect(queryExecutor).Not.To.Be.Null();
                    Expect(tokenService).Not.To.Be.Null();
                }
            }
        }

        [TestFixture]
        public class AuthenticateUser
        {
            [Test]
            public void ShouldGenerateNewJwtToken()
            {
                // arrange
                var ntk8Queries = Substitute.For<IUserQueries>();
                var tokenService = Substitute.For<ITokenService>();
                var user = GetRandom<IUserEntity>();
                var validJwtToken = TestTokenHelpers
                    .CreateValidJwtToken(GetRandomString(40), user.Id);
                var token = new AccessTokenResponse
                {
                    Token = TestTokenHelpers.CreateValidJwtTokenAsString(validJwtToken)
                };
                var authenticateRequest = user.MapFromTo(new AuthenticateRequest());
                authenticateRequest.Password = GetRandomString();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(authenticateRequest.Password);

                ntk8Queries.FetchUserByEmailAddress(Arg.Any<string>()).Returns(user);
                
                tokenService.GenerateJwtToken(user.Id, user.Roles)
                    .Returns(token);

                tokenService.GenerateRefreshToken(user.Id)
                    .Returns(TestTokenHelpers.CreateRefreshToken());

                var accountService = Create(ntk8Queries, null, tokenService);
                // act
                var authenticatedResponse = accountService.AuthenticateUser(authenticateRequest);
                // assert
                Expect(authenticatedResponse.JwtToken).Not.To.Be.Null();
                Expect(authenticatedResponse.JwtToken).To.Equal(token.Token);
            }

            [TestFixture]
            public class WhenUserIsNotVerified
            {
                [Test]
                public void ShouldThrow()
                {
                    // arrange
                    var ntk8Queries = Substitute.For<IUserQueries>();
                    var user = TestUserEntity.Create();
                    user.DateVerified = null;
                    user.DateOfPasswordReset = null;
                    ntk8Queries.FetchUserByEmailAddress(user.Email ?? string.Empty)
                        .Returns(user);
                    var tokenService = Substitute.For<ITokenService>();
                    var authenticateRequest = user.MapFromTo<IUserEntity, AuthenticateRequest>(new AuthenticateRequest());
                    var sut = Create(ntk8Queries, null, tokenService);
                    // act
                    // assert
                    Expect(() => sut.AuthenticateUser(authenticateRequest))
                        .To.Throw<UserIsNotVerifiedException>();
                }
            }

            [Test]
            public void ShouldGenerateNewRefreshToken()
            {
                // arrange
                var ntk8Queries = Substitute.For<IUserQueries>();
                var tokenService = Substitute.For<ITokenService>();
                var user = GetRandom<IUserEntity>();
                var validJwtToken = TestTokenHelpers
                    .CreateValidJwtToken(GetRandomString(40), user.Id);
                
                var resetTokenResponse = new AccessTokenResponse
                {
                    Token = TestTokenHelpers.CreateValidJwtTokenAsString(validJwtToken)
                };
                
                var authenticateRequest = user.MapFromTo(new AuthenticateRequest());
                authenticateRequest.Password = GetRandomString();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(authenticateRequest.Password);

                ntk8Queries.FetchUserByEmailAddress(Arg.Any<string>())
                    .Returns(user);

                tokenService.GenerateJwtToken(Arg.Any<int>(), Arg.Any<Role[]>())
                    .Returns(resetTokenResponse);

                var refreshToken = TestTokenHelpers.CreateRefreshToken();

                tokenService.GenerateRefreshToken(user.Id)
                    .Returns(refreshToken);

                var accountService = Create(ntk8Queries, null, tokenService);
                // act
                accountService.AuthenticateUser(authenticateRequest);
                // assert
                Expect(tokenService)
                    .To.Have.Received(1)
                    .GenerateRefreshToken(user.Id);
            }

            [Test]
            public void ShouldInvalidateOldRefreshToken()
            {
                // arrange
                var ntk8Queries = Substitute.For<IUserQueries>();
                var tokenService = Substitute.For<ITokenService>();
                var user = GetRandom<IUserEntity>();
                var validJwtToken = TestTokenHelpers
                    .CreateValidJwtToken(GetRandomString(40), user.Id);
                
                var resetTokenResponse = new AccessTokenResponse
                {
                    Token = TestTokenHelpers.CreateValidJwtTokenAsString(validJwtToken)
                };
                
                var authenticateRequest = user.MapFromTo(new AuthenticateRequest());
                authenticateRequest.Password = GetRandomString();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(authenticateRequest.Password);

                ntk8Queries.FetchUserByEmailAddress(Arg.Any<string>())
                    .Returns(user);

                tokenService
                    .GenerateJwtToken(Arg.Any<int>(), Arg.Any<Role[]>())
                    .Returns(resetTokenResponse);

                var refreshToken = TestTokenHelpers.CreateRefreshToken();

                tokenService.GenerateRefreshToken(user.Id)
                    .Returns(refreshToken);

                var accountService = Create(ntk8Queries, null, tokenService);
                
                // act
                accountService.AuthenticateUser(authenticateRequest);
                
                // assert
                Expect(tokenService)
                    .To.Have.Received(1)
                    .RevokeRefreshToken(user.RefreshToken);
            }
            
            [Test]
            public void ShouldAttachUserRolesToUserResponse()
            {
                // arrange
                var ntk8Queries = Substitute.For<IUserQueries>();
                var tokenService = Substitute.For<ITokenService>();
                var user = GetRandom<IUserEntity>();
                user.Roles = GetRandomArray<Role>();
                
                var validJwtToken = TestTokenHelpers
                    .CreateValidJwtToken(GetRandomString(40), user.Id);
                
                var resetTokenResponse = new AccessTokenResponse
                {
                    Token = TestTokenHelpers.CreateValidJwtTokenAsString(validJwtToken)
                };
                
                var authenticateRequest = user.MapFromTo(new AuthenticateRequest());
                authenticateRequest.Password = GetRandomString();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(authenticateRequest.Password);

                ntk8Queries.FetchUserByEmailAddress(Arg.Any<string>())
                    .Returns(user);

                tokenService
                    .GenerateJwtToken(Arg.Any<int>(), Arg.Any<Role[]>())
                    .Returns(resetTokenResponse);

                var refreshToken = TestTokenHelpers.CreateRefreshToken();

                tokenService.GenerateRefreshToken(user.Id)
                    .Returns(refreshToken);

                var accountService = Create(ntk8Queries, null, tokenService);
                
                // act
                var response = accountService.AuthenticateUser(authenticateRequest);
                
                // assert
                Expect(response.Roles).To.Equal(user.Roles);
            }

            [Test]
            public void ShouldSetRefreshTokenCookie()
            {
                // arrange
                var ntk8Queries = Substitute.For<IUserQueries>();
                var tokenService = Substitute.For<ITokenService>();
                var user = GetRandom<IUserEntity>();
                user.Roles = GetRandomArray<Role>();
                
                var validJwtToken = TestTokenHelpers
                    .CreateValidJwtToken(GetRandomString(40), user.Id);
                
                var resetTokenResponse = new AccessTokenResponse
                {
                    Token = TestTokenHelpers.CreateValidJwtTokenAsString(validJwtToken)
                };
                
                var authenticateRequest = user.MapFromTo(new AuthenticateRequest());
                authenticateRequest.Password = GetRandomString();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(authenticateRequest.Password);

                ntk8Queries.FetchUserByEmailAddress(Arg.Any<string>())
                    .Returns(user);

                tokenService
                    .GenerateJwtToken(Arg.Any<int>(), Arg.Any<Role[]>())
                    .Returns(resetTokenResponse);

                var refreshToken = TestTokenHelpers.CreateRefreshToken();

                tokenService.GenerateRefreshToken(user.Id)
                    .Returns(refreshToken);

                var accountService = Create(ntk8Queries, null, tokenService);
                
                // act
                var _ = accountService.AuthenticateUser(authenticateRequest);
                
                // assert
                Expect(tokenService)
                    .To.Have
                    .Received(1)
                    .SetRefreshTokenCookie(refreshToken.Token!);
            }

            [TestFixture]
            public class WhenJwtTokensAreEnabled
            {
                [Test]
                public void ShouldReturnJwtTokenWithResponse()
                {
                    // arrange
                    var ntk8Queries = Substitute.For<IUserQueries>();
                    var tokenService = Substitute.For<ITokenService>();
                    var user = GetRandom<IUserEntity>();
                    user.Roles = GetRandomArray<Role>();
                
                    var validJwtToken = TestTokenHelpers
                        .CreateValidJwtToken(GetRandomString(40), user.Id);
                
                    var accessTokenResponse = new AccessTokenResponse
                    {
                        Token = TestTokenHelpers.CreateValidJwtTokenAsString(validJwtToken)
                    };
                
                    var authenticateRequest = user.MapFromTo(new AuthenticateRequest());
                    authenticateRequest.Password = GetRandomString();
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(authenticateRequest.Password);

                    ntk8Queries.FetchUserByEmailAddress(Arg.Any<string>())
                        .Returns(user);

                    tokenService
                        .GenerateJwtToken(Arg.Any<int>(), Arg.Any<Role[]>())
                        .Returns(accessTokenResponse);

                    var refreshToken = TestTokenHelpers.CreateRefreshToken();

                    tokenService.GenerateRefreshToken(user.Id)
                        .Returns(refreshToken);

                    var globalSettings = new GlobalSettings
                    {
                        UseJwt = true
                    };

                    var accountService = Create(
                        ntk8Queries, 
                        null, 
                        tokenService, 
                        null, 
                        globalSettings);

                    // act
                    var result = accountService.AuthenticateUser(authenticateRequest);
                
                    // assert
                    Expect(result.JwtToken).To.Equal(accessTokenResponse.Token);
                }   
            }

            [TestFixture]
            public class WhenJwtTokensAreDisabled
            {
                [Test]
                public void ShouldNotReturnJwtTokenWithResponse()
                {
                    // arrange
                    var ntk8Queries = Substitute.For<IUserQueries>();
                    var tokenService = Substitute.For<ITokenService>();
                    var user = GetRandom<IUserEntity>();
                    user.Roles = GetRandomArray<Role>();
                
                    var validJwtToken = TestTokenHelpers
                        .CreateValidJwtToken(GetRandomString(40), user.Id);
                
                    var accessTokenResponse = new AccessTokenResponse
                    {
                        Token = TestTokenHelpers.CreateValidJwtTokenAsString(validJwtToken)
                    };
                
                    var authenticateRequest = user.MapFromTo(new AuthenticateRequest());
                    authenticateRequest.Password = GetRandomString();
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(authenticateRequest.Password);

                    ntk8Queries.FetchUserByEmailAddress(Arg.Any<string>())
                        .Returns(user);

                    tokenService
                        .GenerateJwtToken(Arg.Any<int>(), Arg.Any<Role[]>())
                        .Returns(accessTokenResponse);

                    var refreshToken = TestTokenHelpers.CreateRefreshToken();

                    tokenService.GenerateRefreshToken(user.Id)
                        .Returns(refreshToken);

                    var globalSettings = new GlobalSettings
                    {
                        UseJwt = false
                    };

                    var accountService = Create(
                        ntk8Queries, 
                        null, 
                        tokenService, 
                        null,
                        globalSettings);

                    // act
                    var result = accountService.AuthenticateUser(authenticateRequest);
                
                    // assert
                    Expect(result.JwtToken).To.Be.Null();
                }
            }
        }

        [TestFixture]
        public class RegisterUser : TestFixtureRequiringServiceProvider
        {
            [TestFixture]
            public class WhenUserDoesExist
            {
                [Test]
                public void ShouldThrowIfUserIsVerifiedAndExists()
                {
                    // arrange
                    var ntk8Queries = Substitute.For<IUserQueries>();
                    var user = GetRandom<IUserEntity>();
                    var registerRequest = user.MapFromTo(new RegisterRequest());
                    ntk8Queries.FetchUserByEmailAddress(user.Email ?? string.Empty).Returns(user);
                    
                    var accountService = Create(ntk8Queries);
                    // act
                    // assert
                    Expect(() => accountService.RegisterUser(registerRequest))
                        .To.Throw<UserAlreadyExistsException>()
                        .With.Message.Containing("User already exists");
                }

                [TestFixture]
                public class WhenUserIsNotVerified
                {
                    [Test]
                    public void ShouldUpdateVerificationToken()
                    {
                        // arrange
                        var ntk8Queries = Substitute.For<IUserQueries>();
                        var ntk8Commands = Substitute.For<IUserCommands>();
                        var user = GetRandom<IUserEntity>();
                        user.DateVerified = null;
                        user.DateOfPasswordReset = null;
                        var registerRequest = user.MapFromTo(new RegisterRequest());

                        ntk8Queries.FetchUserByEmailAddress(Arg.Any<string>()).Returns(user);

                        var accountService = Create(ntk8Queries, ntk8Commands);
                        accountService.RegisterUser(registerRequest);
                        // act
                        // assert
                        Expect(ntk8Commands)
                            .To.Have.Received(1)
                            .UpdateUser(Arg.Is(user));
                    }
                }
            }

            [TestFixture]
            public class WhenUserDoestExist
            {
                [Test]
                public void ShouldSetVerificationToken()
                {
                    // arrange
                    // act
                    // assert
                }

                [TestFixture]
                public class WhenUserHasNotAcceptedTerms
                {
                    [Test]
                    public void ShouldThrow()
                    {
                        // arrange

                        // act
                        // assert
                    }
                }

                [Test]
                public void ShouldHaveAcceptedTerms()
                {
                    // arrange

                    // act
                    // assert
                }
                
                [Test]
                public void ShouldHashUserPassword()
                {
                    // arrange

                    // act
                    // assert
                }
            }
        }

        [TestFixture]
        public class VerifyUserByVerificationToken
        {
        }


        [TestFixture]
        public class ForgotUserPassword
        {
            [Test]
            public void ShouldUpdateResetToken()
            {
                // arrange

                // act
                // assert
            }

            [Test]
            public void ShouldUpdateDateResetTokenExpires()
            {
                // arrange

                // act
                // assert
            }

            [Test]
            public void ShouldReturnResetToken()
            {
                // arrange

                // act
                // assert
            }
        }


        [TestFixture]
        public class ResetUserPassword
        {
            [Test]
            public void ShouldExecuteUpdateUserCommand()
            {
                // arrange
                var user = TestUserEntity.Create();
                user.DateResetTokenExpires = DateTime.UtcNow.AddMinutes(-1);
                var authSettings = GetRandom<AuthSettings>();
                authSettings.PasswordResetTokenTTL = 100;
                var resetPasswordRequest = GetRandom<ResetPasswordRequest>();
                var ntk8Queries = Substitute.For<IUserQueries>();
                var ntk8Commands = Substitute.For<IUserCommands>();
                ntk8Queries.FetchUserByResetToken(resetPasswordRequest.Token!).Returns(user);
                var sut = Create(
                    ntk8Queries: ntk8Queries, 
                    authSettings: authSettings, 
                    ntk8Commands: ntk8Commands);

                // act
                sut.ResetUserPassword(resetPasswordRequest);
                // assert
                Expect(ntk8Commands)
                    .To.Have.Received(1)
                    .UpdateUser(Arg.Is<TestUserEntity>(s => s.Email == user.Email
                                                      && s.ResetToken == null 
                                                      && s.DateResetTokenExpires == null));
            }
        }

        [TestFixture]
        public class GetUserById
        {
            [Test]
            public void ShouldGetUserById()
            {
                // arrange
                
                // act
                // assert
            }
        }

        [TestFixture]
        public class UpdateUserTests
        {
            [Test]
            public void ShouldUpdateUserDetails()
            {
                // arrange
                
                // act
                // assert
            }
        }

        [TestFixture]
        public class DeleteUser
        {
            [Test]
            public void ShouldDeleteUser()
            {
                // arrange
                
                // act
                // assert
            }
        }

        [TestFixture]
        public class AutoVerifyUser
        {
            [Test]
            public void ShouldVerifyUser()
            {
                // arrange
                
                // act
                // assert
            }
        }


        [TestFixture]
        public class ForgotPassword
        {
            [Test]
            public void ShouldGenerateResetTokenForUser()
            {
                // arrange
                
                // act
                // assert
            }
        }

        [TestFixture]
        public class ResetPassword
        {
            [Test]
            public void ShouldSetResetTokenToNull()
            {
                // arrange

                // act
                // assert
            }
        }


        private static AccountService<TestUserEntity> Create(
            IUserQueries? ntk8Queries = null,
            IUserCommands? ntk8Commands = null,
            ITokenService? tokenService = null,
            IAuthSettings? authSettings = null,
            GlobalSettings? globalSettings = null)
        {
            return new AccountService<TestUserEntity>(
                ntk8Commands ?? Substitute.For<IUserCommands>(),
                ntk8Queries ?? Substitute.For<IUserQueries>(),
                tokenService ?? Substitute.For<ITokenService>(),
                authSettings ?? CreateAuthSettings(),
                GetRandom<IUserEntity>(),
                Substitute.For<ILogger<AccountService<TestUserEntity>>>(),
                globalSettings ?? GetRandom<GlobalSettings>(),
                Substitute.For<IHttpContextAccessor>());
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