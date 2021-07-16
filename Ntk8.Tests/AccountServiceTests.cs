using System.Transactions;
using Dapper.CQRS;
using NExpect;
using Ntk8.Dto;
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
                // act
                using (Transactions.RepeatableRead())
                {
                    accountService.Register(registerRequest, origin);
                    // assert
                }
            }
        }

        public IAccountService Create()
        {
            return Resolve<IAccountService>();
        }
    }
}