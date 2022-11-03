using System.Transactions;
using Dapper.CQRS;
using NExpect;
using Ntk8.DatabaseServices;
using Ntk8.Tests.TestHelpers;
using NUnit.Framework;
using static NExpect.Expectations;

namespace Ntk8.Tests.Data.Commands
{
    [TestFixture]
    public class InsertRefreshTokenTests
    {
        [TestFixture]
        public class WhenInsertingNewRefreshTokens : TestFixtureRequiringServiceProvider
        {
            [Test]
            public void ShouldReturnWithBaseUser()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var refreshToken = TestTokenHelpers.CreateRefreshToken();
                    var queryExecutor = Resolve<IQueryExecutor>();
                    var commandExecutor = Resolve<ICommandExecutor>();
                    var userCommands = Resolve<IAccountCommands>();
                    var userQueries = Resolve<IAccountQueries>();

                    var testUser = TestUser.Create();
                    var userId = userCommands.InsertUser(testUser);

                    refreshToken.UserId = userId;
                    // act
                    var refreshTokenId = userCommands.InsertRefreshToken(refreshToken);
                    var baseUser = userQueries.FetchUserByRefreshToken<TestUser>(refreshToken.Token);
                    // assert
                    Expect(baseUser).Not.To.Be.Null();
                }
            }
        }
    }
}