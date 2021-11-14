﻿using System;
using System.Collections.Generic;
using System.Linq;
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
                    var accountService = Resolve<IUserAccountService>();
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
                var user = GetRandom<BaseUser>();
                var token = TokenHelpers
                    .CreateValidJwtToken(GetRandomString(40), user.Id);
                var authenticateRequest = user.MapFromTo<BaseUser, AuthenticateRequest>();
                authenticateRequest.Password = GetRandomString();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(authenticateRequest.Password);

                queryExecutor
                    .Execute(Arg.Is<FetchUserByEmailAddress>(u => u.EmailAddress == user.Email))
                    .Returns(user);

                tokenService.GenerateJwtToken(user.Id, user.Roles.ToArray())
                    .Returns(token);

                tokenService.GenerateRefreshToken()
                    .Returns(TokenHelpers.CreateRandomRefreshToken());
                
                var commandExecutor = Substitute.For<ICommandExecutor>();
                var accountService = Create(queryExecutor, commandExecutor, tokenService);
                // act
                var authenticatedResponse = accountService.AuthenticateUser(authenticateRequest);
                // assert
                Expect(authenticatedResponse.JwtToken).Not.To.Be.Null();
                Expect(authenticatedResponse.JwtToken).To.Equal(token);
            }

            [TestFixture]
            public class WhenUserIsNotVerified
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
            public void ShouldNotAttachUserToHttpContext()
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
                    var user = GetRandom<BaseUser>();
                    var registerRequest = user.MapFromTo<BaseUser, RegisterRequest>();
                    queryExecutor
                        .Execute(Arg.Is<FetchUserByEmailAddress>(u => u.EmailAddress == user.Email))
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
                        var user = GetRandom<BaseUser>();
                        user.DateVerified = null;
                        user.DateOfPasswordReset = null;
                        var registerRequest = user.MapFromTo<BaseUser, RegisterRequest>();
                        queryExecutor
                            .Execute(Arg.Is<FetchUserByEmailAddress>(u => u.EmailAddress == user.Email))
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
        }

        [TestFixture]
        public class GetUserById
        {
        }

        [TestFixture]
        public class UpdateUserTests
        {
        }

        [TestFixture]
        public class DeleteUser
        {
        }

        [TestFixture]
        public class AutoVerifyUser
        {
        }


        [TestFixture]
        public class ForgotPassword
        {
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


        private static IUserAccountService Create(
            IQueryExecutor queryExecutor = null,
            ICommandExecutor commandExecutor = null,
            ITokenService tokenService = null,
            IAuthSettings authSettings = null)
        {
            return new UserAccountService(
                queryExecutor ?? Substitute.For<IQueryExecutor>(),
                commandExecutor ?? Substitute.For<ICommandExecutor>(),
                tokenService ?? Substitute.For<ITokenService>(),
                authSettings ?? Substitute.For<IAuthSettings>());
        }

        private static List<RefreshToken> CreateRefreshTokens(int amount = 3, RefreshToken addTokenToList = null)
        {
            var list = new List<RefreshToken>();
            if (addTokenToList is not null)
            {
                list.Add(addTokenToList);
            }

            amount.Times(() => list.Add(CreateRefreshToken()));
            return list;
        }

        private static RefreshToken CreateRefreshToken(ITokenService tokenService = null)
        {
            tokenService ??= Substitute
                .For<TokenService>(Substitute.For<IQueryExecutor>(),
                    CreateAuthSettings());

            return tokenService.GenerateRefreshToken();
        }

        private static AuthSettings CreateAuthSettings()
        {
            return new()
            {
                RefreshTokenSecret = GetRandomAlphaString(40),
                RefreshTokenTTL = 604800,
                JwtTTL = 1000
            };
        }
    }
}