using System;
using System.Transactions;
using Dapper.CQRS;
using NExpect;
using NExpect.Interfaces;
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
        public class WhenUserHasRefreshToken
        {
            [Test]
            public void ShouldFetchUserAndAddRefreshToken()
            {
                // arrange
                
                // act
                // assert
            }
        }
    }
}