using System;
using Dapper.CQRS;
using NExpect;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Tests.TestHelpers;
using NUnit.Framework;
using static NExpect.Expectations;

namespace Ntk8.Tests.Data.Commands
{
    [TestFixture]
    public class UpdateUserTests : TestFixtureRequiringServiceProvider
    {
        [Test]
        public void UpdateUserShouldUpdateUser()
        {
            using (Transactions.UncommittedRead())
            {
                // arrange
                var user = TestUserEntity.Create();
                var commandExecutor = Resolve<ICommandExecutor>();
                var queryExecutor = Resolve<IQueryExecutor>();
                // act
                var id = commandExecutor.Execute(new InsertUser(user));
                var updatedUser = TestUserEntity.Create();
                updatedUser.UserRoles = null;
                updatedUser.Roles = null;
                updatedUser.RefreshToken = null;
                updatedUser.Id = id;
                commandExecutor.Execute(new UpdateUser(updatedUser));
                var expected = queryExecutor.Execute(new FetchUserById<TestUserEntity>(id));
                Expect(updatedUser.DateModified).To.Approximately.Equal(expected!.DateModified);
                Expect(updatedUser.DateCreated).To.Approximately.Equal(expected.DateCreated);
                Expect(updatedUser.DateVerified).To.Approximately.Equal((DateTime) expected.DateVerified!);
                Expect(updatedUser.DateOfPasswordReset).To.Approximately.Equal((DateTime) expected.DateOfPasswordReset!);
                Expect(updatedUser.DateResetTokenExpires).To.Approximately
                    .Equal((DateTime) expected.DateResetTokenExpires!);
                updatedUser.DateModified = expected.DateModified;
                updatedUser.DateCreated = expected.DateCreated;
                updatedUser.DateVerified = expected.DateVerified;
                updatedUser.DateOfPasswordReset = expected.DateOfPasswordReset;
                updatedUser.DateResetTokenExpires = expected.DateResetTokenExpires;
                expected.Roles = null;
                // assert
                Expect(updatedUser).To.Deep.Equal(expected);
            }
        }
    }
}