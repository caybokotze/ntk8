using System;
using System.Transactions;
using Dapper.CQRS;
using NExpect;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Models;
using Ntk8.Tests.TestHelpers;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Data.Queries;

[TestFixture]
public class FetchUserByResetTokenTests
{
    [TestFixture]
    public class WhenFetchingSingleResetToken : TestFixtureRequiringServiceProvider
    {
        [Test]
        public void ShouldFetchExpectedUser()
        {
            using (new TransactionScope())
            {
                // arrange
                var queryExecutor = Resolve<IQueryExecutor>();
                var commandExecutor = Resolve<ICommandExecutor>();

                var user = TestUser.Create();
                user.RefreshToken = null;
                user.UserRoles = null;
                user.Roles = Array.Empty<Role>();
                user.ResetToken = GetRandomAlphaString();
                var userId = commandExecutor.Execute(new InsertUser(user));
                user.Id = userId;
                // act
                var expectedUser = queryExecutor.Execute(new FetchUserByResetToken<TestUser>(user.ResetToken));
                // assert
                Expect(expectedUser!.DateCreated).To.Approximately.Equal(user.DateCreated);
                Expect(expectedUser.DateModified).To.Approximately.Equal(user.DateModified);
                Expect(expectedUser.DateVerified).To.Approximately.Equal(user.DateVerified!.Value);
                Expect(expectedUser.DateOfPasswordReset).To.Approximately.Equal(user.DateOfPasswordReset!.Value);
                Expect(expectedUser.DateResetTokenExpires).To.Approximately.Equal(user.DateResetTokenExpires!.Value);
                expectedUser.DateCreated = user.DateCreated;
                expectedUser.DateModified = user.DateModified;
                expectedUser.DateVerified = user.DateVerified;
                expectedUser.DateOfPasswordReset = user.DateOfPasswordReset;
                expectedUser.DateResetTokenExpires = user.DateResetTokenExpires;
                Expect(expectedUser).To.Intersection.Equal(user);
            }
        }
    }
}