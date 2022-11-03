using System;
using System.Transactions;
using NExpect;
using Ntk8.DatabaseServices;
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
            var queryExecutor = Resolve<IAccountQueries>();
            var commandExecutor = Resolve<IAccountCommands>();
            var refreshToken = TestTokenHelpers.CreateRefreshToken();
            var user = TestUser.Create();
            
            // act
            var userid = commandExecutor.InsertUser(user);
            refreshToken.UserId = userid;

            var refreshTokenId = commandExecutor.InsertRefreshToken(refreshToken);

            queryExecutor.FetchUserByRefreshToken<TestUser>(refreshToken.Token);
            
            commandExecutor.UpdateRefreshToken(refreshToken);

            var secondRetrievedUser = queryExecutor.FetchUserByRefreshToken<TestUser>(refreshToken.Token);

            var expectedRefreshToken = secondRetrievedUser?.RefreshToken;
            
            // assert
            Expect(expectedRefreshToken?.Expires)
                .To.Approximately
                .Equal(DateTime.UtcNow.AddDays(1));
            
            Expect(expectedRefreshToken?.RevokedByIp).To.Equal(refreshToken.RevokedByIp);
            Expect(expectedRefreshToken?.DateRevoked).To.Approximately.Equal(DateTime.UtcNow.AddMinutes(5));
            Expect(expectedRefreshToken?.Id).To.Equal(refreshTokenId);
            Expect(expectedRefreshToken?.CreatedByIp).To.Equal(refreshToken.CreatedByIp);
            Expect(expectedRefreshToken?.DateCreated).To.Approximately.Equal(refreshToken.DateCreated);
        }

        [Test]
        public void ShouldOnlyUpdateExpectedToken()
        {
            using var scope = new TransactionScope();
            
            // arrange
            var queryExecutor = Resolve<IAccountQueries>();
            var commandExecutor = Resolve<IAccountCommands>();
            var refreshToken1 = TestTokenHelpers.CreateRefreshToken();
            var refreshToken2 = TestTokenHelpers.CreateRefreshToken();
            refreshToken2.DateCreated = DateTime.UtcNow.AddDays(1);
            var user = TestUser.Create();
            
            // act
            var userid = commandExecutor.InsertUser(user);
            refreshToken1.UserId = userid;
            refreshToken2.UserId = userid;

            var _ = commandExecutor.InsertRefreshToken(refreshToken1);
            var __ = commandExecutor.InsertRefreshToken(refreshToken2);
            
            var initialRetrievedUser = queryExecutor.FetchUserByRefreshToken<TestUser>(refreshToken1.Token);

            
            refreshToken1.DateRevoked = DateTime.UtcNow.AddMinutes(5);
            refreshToken1.Expires = DateTime.UtcNow.AddDays(1);
            refreshToken1.RevokedByIp = RandomValueGen.GetRandomIPv4Address();
            
            commandExecutor.UpdateRefreshToken(refreshToken1);

            var secondRetrievedUser =
                queryExecutor.FetchUserByRefreshToken<TestUser>(refreshToken1.Token);

            var thirdRetrievedUser = queryExecutor.FetchUserByRefreshToken<TestUser>(refreshToken2.Token);

            var expectedRefreshToken = secondRetrievedUser.RefreshToken;
            var secondRefreshToken = thirdRetrievedUser.RefreshToken;
            
            // assert
            Expect(expectedRefreshToken?.Token).To.Equal(refreshToken1.Token);
            Expect(secondRefreshToken?.Token).To.Equal(refreshToken2.Token);
            Expect(expectedRefreshToken?.Token).Not.To.Equal(refreshToken2.Token);
        }
    }
}