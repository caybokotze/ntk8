using System;
using System.Transactions;
using Dapper.CQRS;
using NExpect;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Tests.TestHelpers;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using static NExpect.Expectations;

namespace Ntk8.Tests.Data.Commands;

[TestFixture]
public class UpdateRefreshTokenTests
{
    [TestFixture]
    public class Transactions : TestFixtureRequiringServiceProvider
    {
        [Test]
        public void ShouldUpdateRefreshToken()
        {
            using var scope = new TransactionScope();
            // arrange
            var queryExecutor = Resolve<IQueryExecutor>();
            var commandExecutor = Resolve<ICommandExecutor>();
            var refreshToken = TestTokenHelpers.CreateRefreshToken();
            var user = TestUser.Create();
            // act
            var userid = commandExecutor.Execute(new InsertUser(user));
            refreshToken.UserId = userid;
            var _ = commandExecutor.Execute(new InsertRefreshToken(refreshToken));
            var initialRetrievedUser = queryExecutor
                .Execute(new FetchUserByRefreshToken<TestUser>(refreshToken.Token ?? string.Empty));

            if (initialRetrievedUser is not null)
            {
                refreshToken.DateRevoked = DateTime.UtcNow.AddMinutes(5);
                refreshToken.Expires = DateTime.UtcNow.AddDays(1);
                refreshToken.RevokedByIp = RandomValueGen.GetRandomIPv4Address();
            }
            
            commandExecutor.Execute(new UpdateRefreshToken(refreshToken));
            
            var secondRetrievedUser =
                queryExecutor.Execute(new FetchUserByRefreshToken<TestUser>(refreshToken.Token ?? string.Empty));

            var expectedRefreshToken = secondRetrievedUser?.RefreshToken;
            // assert
            Expect(expectedRefreshToken?.Expires)
                .To.Approximately
                .Equal(DateTime.UtcNow.AddMinutes(30));
        }

        [Test]
        public void ShouldOnlyUpdateExpectedToken()
        {
            // arrange
            
            // act
            // assert
        }
    }
}