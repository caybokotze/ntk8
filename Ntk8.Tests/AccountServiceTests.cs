using System.Transactions;
using Dapper.CQRS;
using HigLabo.Core;
using NExpect;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Dto;
using Ntk8.Models;
using Ntk8.Services;
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
        public class DatabaseTests : AccountServiceTests
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
                    var user = queryExecutor.Execute(new FetchUserByEmailAddress(registerRequest.Email));
                    var result = user.Map(new RegisterRequest());
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
                var origin = GetRandomIPv4Address();
                var updateUser = GetRandom<UpdateRequest>();
                var queryExecutor = Resolve<IQueryExecutor>();
                var commandExecutor = Resolve<ICommandExecutor>();
                // act
                using (Transactions.RepeatableRead())
                {
                    var userId = commandExecutor.Execute(new InsertUser(GetRandom<User>()));
                    accountService.Update(userId, updateUser);
                    var user = queryExecutor.Execute(new FetchUserById(userId));
                    // assert
                    Expect(user.IsActive).To.Be.True();
                }
            }
        }

        public IAccountService Create(bool mocked = false)
        {
            if (mocked)
            {
                    
            }
            
            return Resolve<IAccountService>();
        }
    }
}