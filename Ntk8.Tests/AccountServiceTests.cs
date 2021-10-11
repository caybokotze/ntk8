using Dapper.CQRS;
using HigLabo.Core;
using NExpect;
using NSubstitute;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Dto;
using Ntk8.Models;
using Ntk8.Services;
using Ntk8.Tests.Helpers;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests
{
    [TestFixture]
    public class AccountServiceTests : TestBase
    {
        [Test]
        public void PrimaryServicesShouldNotBeNull()
        {
            // arrange
            var accountService = Resolve<IAccountService>();
            var commandExecutor = Resolve<ICommandExecutor>();
            var queryExecutor = Resolve<IQueryExecutor>();
            // act
            // assert
            Expect(accountService).Not.To.Be.Null();
            Expect(commandExecutor).Not.To.Be.Null();
            Expect(queryExecutor).Not.To.Be.Null();
        }
        
        [TestFixture]
        public class IntegrationTests : TestBase
        {
            [Test]
            public void RegisterShouldRegisterUser()
            {
                // arrange
                var accountService = Create();
                var origin = GetRandomIPv4Address();
                var registerRequest = GetRandom<RegisterRequest>();
                registerRequest.Email = GetRandomEmail();
                var queryExecutor = Resolve<IQueryExecutor>();
                // act
                using (Transactions.RepeatableRead())
                {
                    accountService.Register(registerRequest, origin);
                    // assert
                    var user = queryExecutor
                        .Execute(new FetchUserByEmailAddress(registerRequest.Email));
                    var result = user
                        .Map(new RegisterRequest());
                    result.Password = registerRequest.Password;
                    Expect(result).Not.To.Be.Null();
                    Expect(result).To.Deep.Equal(registerRequest);
                }
            }

            [Test]
            public void RegisteredUserShouldBeActive()
            {
                // arrange
                var accountService = Create();
                var origin = GetRandomIPv4Address();
                var registerRequest = GetRandom<RegisterRequest>();
                registerRequest.Email = GetRandomEmail();
                var queryExecutor = Resolve<IQueryExecutor>();
                // act
                using (Transactions.UncommittedRead())
                {
                    accountService.Register(registerRequest, origin);
                    var user = queryExecutor.Execute(new FetchUserByEmailAddress(registerRequest.Email));
                    var result = user.Map(new RegisterRequest());
                    result.Password = registerRequest.Password;
                    // assert
                    Expect(user.IsActive).To.Be.True();
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
                    var userToMatch = user.Map(new UpdateRequest());
                    // assert
                    Expect(userToMatch).To.Deep.Equal(updatedUser);
                }
            }
        }

        [TestFixture]
        public class Behaviour
        {
            [TestFixture]
            public class RevokeAndGenerateRefreshToken
            {
                [TestFixture]
                public class WhenUserHasExistingRefreshTokens
                {
                    [Test]
                    public void ShouldGenerateNewRefreshToken()
                    {
                        // arrange
                        
                        // act
                        // assert
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

                [Test]
                public void ShouldGenerateNewRefreshToken()
                {
                    // arrange
                    var accountService = Create();
                    var token = TokenHelpers.CreateValidJwtToken();
                    var ipAddress = GetRandomIPv4Address();
                    // act
                    var authenticatedResponse = accountService
                        .RevokeAndGenerateRefreshToken(token, ipAddress);
                    // assert
                    
                }
            }
        }

        public static IAccountService Create(
            IQueryExecutor queryExecutor = null,
            ICommandExecutor commandExecutor = null,
            IAuthenticationContextService authenticationContextService = null,
            AuthSettings authSettings = null)
        {
            return new AccountService(
                queryExecutor ?? Substitute.For<IQueryExecutor>(),
                commandExecutor ?? Substitute.For<ICommandExecutor>(),
                authenticationContextService ?? Substitute.For<IAuthenticationContextService>(),
                authSettings ?? GetRandom<AuthSettings>());
        }
    }
}