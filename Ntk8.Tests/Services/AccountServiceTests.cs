using System;
using System.Collections.Generic;
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
using PeanutButter.Utils;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Services
{
    [TestFixture]
    public class AccountServiceTests : TestFixtureWithServiceProvider
    {
        [Test]
        public void PrimaryServicesShouldNotBeNull()
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

        [TestFixture]
        public class IntegrationTests : TestFixtureWithServiceProvider
        {
            [TestFixture]
            public class WhenRegisteringUser : TestFixtureWithServiceProvider
            {
                [Test]
                public void ShouldBeActiveUser()
                {
                    using (new TransactionScope())
                    {
                        // arrange
                        var registerRequest = GetRandom<RegisterRequest>();
                        registerRequest.Email = GetRandomEmail();
                        var queryExecutor = Resolve<IQueryExecutor>();
                        var commandExecutor = Resolve<ICommandExecutor>();
                        var accountService = Create(queryExecutor, commandExecutor);
                        // act
                        accountService.Register(registerRequest);
                        var user = queryExecutor.Execute(new FetchUserByEmailAddress(registerRequest.Email));
                        // assert
                        Expect(user.IsActive).To.Be.True();
                    }
                }
            }

            [Test]
            public void UpdateShouldUpdateUser()
            {
                // arrange
                var accountService = Create();
                var updatedUser = GetRandom<UpdateRequest>();
                var queryExecutor = Resolve<IQueryExecutor>();
                var commandExecutor = Resolve<ICommandExecutor>();
                // act
                using (Transactions.RepeatableRead())
                {
                    var userId = commandExecutor.Execute(new InsertUser(GetRandom<BaseUser>()));
                    accountService.Update(userId, updatedUser);
                    var user = queryExecutor.Execute(new FetchUserById(userId));
                    var userToMatch = user.MapFromTo<BaseUser, UpdateRequest>();
                    // assert
                    Expect(userToMatch).To.Deep.Equal(updatedUser);
                }
            }
        }

        [TestFixture]
        public class WhenRegisteringUsers
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
                    Expect(() => accountService.Register(registerRequest))
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
                        accountService.Register(registerRequest);
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
            }
           
        }

        [TestFixture]
        public class Behaviour
        {
            [TestFixture]
            public class Authenticate
            {
                [Test]
                public void ShouldGenerateRefreshToken()
                {
                    // arrange

                    // act
                    // assert
                }

                [Test]
                public void ShouldGenerateJwtToken()
                {
                    // arrange

                    // act
                    // assert
                }

                [TestFixture]
                public class WhenFetchingUser
                {
                    [Test]
                    public void ShouldThrowWhenUserNotFound()
                    {
                        // arrange

                        // act
                        // assert
                    }
                }

                [TestFixture]
                public class WhenValidatingPassword
                {
                    [Test]
                    public void ShouldThrowWhenPasswordDoesntMatchHash()
                    {
                        // arrange

                        // act
                        // assert
                    }
                }
            }

            [TestFixture]
            public class RevokeRefreshTokenAndGenerateNewRefreshToken
            {
                [TestFixture]
                public class WhenUserHasExistingRefreshTokens : TestFixtureWithServiceProvider
                {
                    [Test]
                    public void ShouldGenerateNewRefreshToken()
                    {
                        // arrange
                        var token = CreateRefreshToken();
                        var user = TestUser.Create();
                        user.RefreshTokens = CreateRefreshTokens(2, token);
                        var queryExecutor = Substitute.For<IQueryExecutor>();
                        var commandExecutor = Substitute.For<ICommandExecutor>();
                        var tokenService = Substitute.For<ITokenService>();
                        var newToken = CreateRefreshToken();
                        tokenService
                            .GenerateRefreshToken()
                            .Returns(newToken);
                        tokenService
                            .FetchUserAndCheckIfRefreshTokenIsActive(token.Token)
                            .Returns(user);
                        queryExecutor
                            .Execute(Arg.Is<FetchUserByRefreshToken>(f => f.Token == token.Token))
                            .Returns(user);
                        // act
                        var accountService = Create(queryExecutor, commandExecutor, tokenService);
                        var authenticatedResponse = accountService
                            .GenerateNewRefreshToken(token.Token);
                        // assert
                        Expect(authenticatedResponse).Not.To.Be.Null();
                        // todo: see if refresh token is set in context.
                    }

                    [Test]
                    public void ShouldCallRevokeTokenAndReturnUserToBeCalled()
                    {
                        // arrange

                        // act
                        // assert
                    }
                }

                [TestFixture]
                public class WhenUserDoesNotHaveExistingRefreshTokens
                {
                    [Test]
                    public void ShouldGenerateNewRefreshToken()
                    {
                        // arrange

                        // act
                        // assert
                    }
                }

                [TestFixture]
                public class WhenGeneratingNewRefreshToken
                {
                    [Test]
                    public void ShouldInsertNewRefreshToken()
                    {
                        // arrange
                        var token = CreateRefreshToken();
                        var user = TestUser.Create();
                        user.RefreshTokens = CreateRefreshTokens(2, token);
                        var queryExecutor = Substitute.For<IQueryExecutor>();
                        var commandExecutor = Substitute.For<ICommandExecutor>();
                        var tokenService = Substitute.For<ITokenService>();
                        var newToken = CreateRefreshToken();
                        tokenService
                            .GenerateRefreshToken()
                            .Returns(newToken);
                        tokenService
                            .FetchUserAndCheckIfRefreshTokenIsActive(token.Token)
                            .Returns(user);
                        queryExecutor
                            .Execute(Arg.Is<FetchUserByRefreshToken>(f => f.Token == token.Token))
                            .Returns(user);
                        // act
                        var accountService = Create(queryExecutor, commandExecutor, tokenService);
                        accountService
                            .GenerateNewRefreshToken(token.Token);
                        // assert
                        Expect(commandExecutor)
                            .To.Have
                            .Received(1)
                            .Execute(Arg.Is<InsertRefreshToken>(r => r.RefreshToken == newToken));
                    }
                }

                [Test]
                public void ShouldGenerateNewRefreshTokenAndRevokeOldOnes()
                {
                    // arrange
                    var token = CreateRefreshToken();
                    var user = TestUser.Create();
                    user.RefreshTokens = CreateRefreshTokens(2, token);
                    user.RefreshTokens.ForEach(f =>
                    {
                        f.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                    });
                    var queryExecutor = Substitute.For<IQueryExecutor>();
                    var commandExecutor = Substitute.For<ICommandExecutor>();
                    var tokenService = Substitute.For<ITokenService>();
                    var newToken = CreateRefreshToken();
                    tokenService
                        .GenerateRefreshToken()
                        .Returns(newToken);
                    tokenService
                        .FetchUserAndCheckIfRefreshTokenIsActive(token.Token)
                        .Returns(user);
                    queryExecutor
                        .Execute(Arg.Is<FetchUserByRefreshToken>(f => f.Token == token.Token))
                        .Returns(user);
                    // act
                    var accountService = Create(queryExecutor, commandExecutor, tokenService);
                    accountService
                        .GenerateNewRefreshToken(token.Token);
                    // assert
                    Expect(commandExecutor)
                        .To.Have
                        .Received(1)
                        .Execute(Arg.Is<InsertRefreshToken>(r => r.RefreshToken == newToken));
                }
            }
            
            [TestFixture]
            public class RevokeRefreshToken
            {
                [Test]
                public void ShouldRevokeRefreshToken()
                {
                    // arrange
                    var commandExecutor = Substitute.For<ICommandExecutor>();
                    var queryExecutor = Substitute.For<IQueryExecutor>();
                    var testUser = TestUser.Create();
                    var refreshToken = CreateRefreshToken();
                    testUser.RefreshTokens.Add(refreshToken);

                    commandExecutor
                        .Execute(Arg.Any<UpdateRefreshToken>())
                        .Returns(1);

                    // act
                    var accountService = Create(queryExecutor, commandExecutor);

                    // TODO: FIXME
                    // accountService
                    //     .RevokeRefreshToken(refreshToken);
                    
                    // assert
                    Expect(commandExecutor)
                        .To.Have.Received(1)
                        .Execute(Arg.Is<UpdateRefreshToken>(u => u.RefreshToken == refreshToken));
                }
            }
        }

        private static IAccountService Create(
            IQueryExecutor queryExecutor = null,
            ICommandExecutor commandExecutor = null,
            ITokenService tokenService = null,
            IAuthSettings authSettings = null)
        {
            return new AccountService(
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