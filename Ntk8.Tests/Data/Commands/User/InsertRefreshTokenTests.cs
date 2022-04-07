using System.Transactions;
using Dapper.CQRS;
using NExpect;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Tests.Helpers;
using NUnit.Framework;
using static NExpect.Expectations;

namespace Ntk8.Tests.Data.Commands.User
{
    [TestFixture]
    public class InsertRefreshTokenTests
    {
        [TestFixture]
        public class WhenInsertingNewRefreshTokens : TestFixtureWithServiceProvider
        {
            [Test]
            public void ShouldReturnWithBaseUser()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var refreshToken = TokenHelpers.CreateRefreshToken();
                    var queryExecutor = Resolve<IQueryExecutor>();
                    var commandExecutor = Resolve<ICommandExecutor>();

                    var testUser = TestUser.Create();
                    var userId = commandExecutor.Execute(new InsertUser(testUser));

                    refreshToken.UserId = userId;
                    // act
                    commandExecutor.Execute(new InsertRefreshToken(refreshToken));
                    var baseUser = queryExecutor.Execute(new FetchUserByRefreshToken(refreshToken.Token));
                    // assert
                    Expect(baseUser).Not.To.Be.Null();
                }
            }
        }
    }
}