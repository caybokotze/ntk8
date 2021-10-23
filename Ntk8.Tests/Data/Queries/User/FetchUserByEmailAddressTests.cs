using System;
using System.Transactions;
using Dapper.CQRS;
using NExpect;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Models;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using static NExpect.Expectations;

namespace Ntk8.Tests.Data.Queries.User
{
    [TestFixture]
    public class FetchUserByEmailAddressTests
    {
        [TestFixture]
        public class Transactional : TestFixtureWithServiceProvider
        {
            [Test]
            public void ShouldFetchAllUserProperties()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var queryExecutor = Resolve<IQueryExecutor>();
                    var commandExecutor = Resolve<ICommandExecutor>();
                    var user = RandomValueGen.GetRandom<BaseUser>();
                    // act
                    var userId = commandExecutor.Execute(new InsertUser(user));
                    var expectedUser = queryExecutor.Execute(new FetchUserByEmailAddress(user.Email));
                    user.Id = userId;
                    // assert
                    Expect(user.DateCreated).To.Approximately.Equal(expectedUser.DateCreated);
                    Expect(user.DateModified).To.Approximately.Equal(expectedUser.DateModified);
                    Expect(user.DateVerified).To.Approximately
                        .Equal((DateTime)expectedUser.DateVerified);
                    Expect(user.DateOfPasswordReset).To.Approximately
                        .Equal((DateTime)expectedUser.DateOfPasswordReset);
                    Expect(user.DateResetTokenExpires).To.Approximately
                        .Equal((DateTime) expectedUser.DateResetTokenExpires);
                    user.DateCreated = expectedUser.DateCreated;
                    user.DateModified = expectedUser.DateModified;
                    user.DateVerified = expectedUser.DateVerified;
                    user.DateOfPasswordReset = expectedUser.DateOfPasswordReset;
                    user.DateResetTokenExpires = expectedUser.DateResetTokenExpires;
                    Expect(user)
                        .To.Deep.Equal(expectedUser);
                }
            }   
        }
    }
}