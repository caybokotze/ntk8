using System;
using Dapper.CQRS;
using NExpect;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Models;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using static NExpect.Expectations;

namespace Ntk8.Tests.Commands
{
    [TestFixture]
    public class WhenInsertingUsers : TestFixtureWithServiceProvider
    {
        [Test]
        public void ShouldInsertUser()
        {
            // arrange
            var user = RandomValueGen.GetRandom<BaseUser>();
            var commandExecutor = Resolve<ICommandExecutor>();
            var queryExecutor = Resolve<IQueryExecutor>();
            // act
            using (Transactions.UncommittedRead())
            {
                var id = commandExecutor
                    .Execute(new InsertUser(user));
                var expectedUser = queryExecutor
                    .Execute(new FetchUserById(id)); 
                // assert
                Expect(id)
                    .To.Be.Greater.Than(1);
                expectedUser.DateCreated = user.DateCreated;
                expectedUser.DateModified = user.DateModified;
                expectedUser.DateVerified = user.DateVerified;
                expectedUser.DateOfPasswordReset = user.DateOfPasswordReset;
                expectedUser.DateResetTokenExpires = user.DateResetTokenExpires;
                user.RefreshTokens = null;
                user.UserRoles = null;
                user.Id = id;
                Expect(expectedUser)
                    .To.Deep.Equal(user);
            }
        }

        [Test]
        public void ShouldReturnExpectedDatesAndNotBeNull()
        {
            // arrange
            var user = RandomValueGen.GetRandom<BaseUser>();
            var commandExecutor = Resolve<ICommandExecutor>();
            var queryExecutor = Resolve<IQueryExecutor>();
            // act
            using (Transactions.UncommittedRead())
            {
                var id = commandExecutor
                    .Execute(new InsertUser(user));

                var expectedUser = queryExecutor
                    .Execute(new FetchUserById(id));
                // assert
                Expect(expectedUser.DateCreated)
                    .To.Approximately.Equal(user.DateCreated);
                Expect(expectedUser.DateModified)
                    .To.Approximately.Equal(user.DateModified);
                
                if (user.DateVerified is not null)
                {
                    Expect(expectedUser.DateVerified)
                        .To.Approximately.Equal((DateTime) user.DateVerified);
                }

                if (user.DateOfPasswordReset is not null)
                {
                    Expect(expectedUser.DateOfPasswordReset)
                        .To.Approximately.Equal((DateTime) user.DateOfPasswordReset);
                }

                if (user.DateResetTokenExpires is not null)
                {
                    Expect(expectedUser.DateResetTokenExpires)
                        .To.Approximately.Equal((DateTime) user.DateResetTokenExpires);
                }

                Expect(user.DateVerified).To.Not.Be.Null();
                Expect(user.DateOfPasswordReset).To.Not.Be.Null();
                Expect(user.DateResetTokenExpires).To.Not.Be.Null();
            }
        }
    }
}