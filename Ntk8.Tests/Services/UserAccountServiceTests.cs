using System;
using System.Transactions;
using Dapper.CQRS;
using NExpect;
using NSubstitute;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Dto;
using Ntk8.Exceptions;
using Ntk8.Helpers;
using Ntk8.Models;
using Ntk8.Services;
using Ntk8.Tests.Helpers;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Services
{
    [TestFixture]
    public class UserAccountServiceTests
    {
        [TestFixture]
        public class DependencyInjection : TestFixtureWithServiceProvider
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
                var queryExecutor = Substitute.For<IQueryExecutor>();
                var tokenService = Substitute.For<ITokenService>();
                var user = GetRandom<IBaseUser>();
                var validJwtToken = TokenHelpers
                    .CreateValidJwtToken(GetRandomString(40), user.Id);
                var token = new ResetTokenResponse
                {
                    Token = TokenHelpers.CreateValidJwtTokenAsString(validJwtToken)
                };
                var authenticateRequest = user.MapFromTo(new AuthenticateRequest());
                authenticateRequest.Password = GetRandomString();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(authenticateRequest.Password);

                queryExecutor
                    .Execute(Arg.Is<FetchUserByEmailAddress<TestUser>>(u => u.EmailAddress == user.Email))
                    .Returns(user);
                
                tokenService.GenerateJwtToken(user.Id, user.Roles)
                    .Returns(token);

                tokenService.GenerateRefreshToken()
                    .Returns(TokenHelpers.CreateRefreshToken());
                
                var commandExecutor = Substitute.For<ICommandExecutor>();
                
                var accountService = Create(queryExecutor, commandExecutor, tokenService);
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
                    var queryExecutor = Substitute.For<IQueryExecutor>();
                    var user = TestUser.Create();
                    user.DateVerified = null;
                    user.DateOfPasswordReset = null;
                    queryExecutor.Execute(Arg.Is<FetchUserByEmailAddress<TestUser>>(u => u.EmailAddress == user.Email))
                        .Returns(user);
                    var tokenService = Substitute.For<ITokenService>();
                    var authenticateRequest = user.MapFromTo<IBaseUser, AuthenticateRequest>(new AuthenticateRequest());
                    var sut = Create(queryExecutor, null, tokenService);
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

                // act
                // assert
            }

            [Test]
            public void ShouldFetchTheUserFromDb()
            {
                // arrange

                // act
                // assert
            }

            [Test]
            public void ShouldAttachUserRolesToUserResponse()
            {
                // arrange

                // act
                // assert
            }

            [Test]
            public void ShouldNotUpdateUser()
            {
                // arrange
                
                // act
                // assert
            }

            [Test]
            public void ShouldRespondWithUserRoles()
            {
                // arrange
                
                // act
                // assert
            }

            [Test]
            public void ShouldSetRefreshTokenCookie()
            {
                // arrange
                
                // act
                // assert
            }
        }

        [TestFixture]
        public class RegisterUser : TestFixtureWithServiceProvider
        {
            [TestFixture]
            public class WhenUserDoesExist
            {
                [Test]
                public void ShouldThrowIfUserIsVerifiedAndExists()
                {
                    // arrange
                    var queryExecutor = Substitute.For<IQueryExecutor>();
                    var user = GetRandom<IBaseUser>();
                    var registerRequest = user.MapFromTo(new RegisterRequest());
                    queryExecutor
                        .Execute(Arg.Is<FetchUserByEmailAddress<TestUser>>(u => u.EmailAddress == user.Email))
                        .Returns(user);
                    var commandExecutor = Substitute.For<ICommandExecutor>();
                    var accountService = Create(queryExecutor, commandExecutor);
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
                        var queryExecutor = Substitute.For<IQueryExecutor>();
                        var commandExecutor = Substitute.For<ICommandExecutor>();
                        var user = GetRandom<IBaseUser>();
                        user.DateVerified = null;
                        user.DateOfPasswordReset = null;
                        var registerRequest = user.MapFromTo(new RegisterRequest());
                        queryExecutor
                            .Execute(Arg.Is<FetchUserByEmailAddress<TestUser>>(u => u.EmailAddress == user.Email))
                            .Returns(user);
                        var ipAddress = GetRandomIPv4Address();
                        var accountService = Create(queryExecutor, commandExecutor);
                        accountService.RegisterUser(registerRequest);
                        // act
                        // assert
                        Expect(commandExecutor)
                            .To.Have.Received(1)
                            .Execute(Arg.Is<UpdateUser>(u => u.BaseUser.DateModified.Truncate(TimeSpan.FromMinutes(1))
                                .Equals(DateTime.UtcNow.Truncate(TimeSpan.FromMinutes(1)))));
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
            public void ShouldResetUserPassword()
            {
                // arrange
                
                // act
                // assert
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


        private static IAccountService Create(
            IQueryExecutor? queryExecutor = null,
            ICommandExecutor? commandExecutor = null,
            ITokenService? tokenService = null,
            IAuthSettings? authSettings = null,
            Ntk8CustomSqlStatements? statements = null)
        {
            return new AccountService<TestUser>(
                queryExecutor ?? Substitute.For<IQueryExecutor>(),
                commandExecutor ?? Substitute.For<ICommandExecutor>(),
                tokenService ?? Substitute.For<ITokenService>(),
                authSettings ?? CreateAuthSettings(),
                GetRandom<IBaseUser>(), statements);
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