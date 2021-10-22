using System;
using System.Linq;
using System.Transactions;
using Dapper.CQRS;
using NExpect;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Models;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Data.Queries.User
{
    [TestFixture]
    public class FetchUserByIdTests
    {
        [TestFixture]
        public class WhenFetchingUser : TestFixtureWithServiceProvider
        {
            [Test]
            public void ShouldFetchAllProperties()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var commandExecutor = Resolve<ICommandExecutor>();
                    var queryExecutor = Resolve<IQueryExecutor>();
                    // act
                    var expectedUser = GetRandom<BaseUser>();
                    expectedUser.RefreshTokens = null;
                    var userId = commandExecutor.Execute(new InsertUser(expectedUser));
                    var actualUser = queryExecutor.Execute(new FetchUserById(userId));
                    expectedUser.Id = userId;
                    actualUser.RefreshTokens = null;
                    // assert
                    Expect(actualUser.DateCreated)
                        .To.Approximately.Equal(expectedUser.DateCreated);
                    Expect(actualUser.DateModified)
                        .To.Approximately.Equal(expectedUser.DateModified);
                    Expect(actualUser.DateVerified)
                        .To.Approximately.Equal((DateTime) expectedUser.DateVerified);
                    Expect(actualUser.DateOfPasswordReset)
                        .To.Approximately.Equal((DateTime) expectedUser.DateOfPasswordReset);
                    Expect(actualUser.DateResetTokenExpires)
                        .To.Approximately.Equal((DateTime) expectedUser.DateResetTokenExpires);
                    expectedUser.DateCreated = actualUser.DateCreated;
                    expectedUser.DateModified = actualUser.DateModified;
                    expectedUser.DateVerified = actualUser.DateVerified;
                    expectedUser.DateOfPasswordReset = actualUser.DateOfPasswordReset;
                    expectedUser.DateResetTokenExpires = actualUser.DateResetTokenExpires;
                    Expect(actualUser).To.Deep.Equal(expectedUser);
                }
            }
        }

        [TestFixture]
        public class WhenUserHasRefreshToken : TestFixtureWithServiceProvider
        {
            [Test]
            public void ShouldFetchUserAndAddRefreshToken()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var commandExecutor = Resolve<ICommandExecutor>();
                    var queryExecutor = Resolve<IQueryExecutor>();
                    var user = GetRandom<BaseUser>();
                    var refreshToken = GetRandom<Ntk8.Models.RefreshToken>();
                    refreshToken.BaseUser = null;
                    // act
                    var userId = commandExecutor
                        .Execute(new InsertUser(user));
                    refreshToken.UserId = userId;
                    var refreshTokenId = commandExecutor
                        .Execute(new InsertRefreshToken(refreshToken));
                    var expectedUser = queryExecutor
                        .Execute(new FetchUserById(userId));
                    refreshToken.Id = refreshTokenId;
                    // assert
                    var expectedToken = expectedUser.RefreshTokens.First();
                    Expect(expectedToken.Expires)
                        .To.Approximately.Equal(refreshToken.Expires);
                    Expect(expectedToken.DateCreated)
                        .To.Approximately.Equal(refreshToken.DateCreated);
                    Expect(expectedToken.DateRevoked)
                        .To.Approximately.Equal((DateTime)refreshToken.DateRevoked);
                    expectedToken.Expires = refreshToken.Expires;
                    expectedToken.DateCreated = refreshToken.DateCreated;
                    expectedToken.DateRevoked = refreshToken.DateRevoked;
                    Expect(expectedToken)
                        .To.Deep.Equal(refreshToken);
                }
            }
        }
    }
}