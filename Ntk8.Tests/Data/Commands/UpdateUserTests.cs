using System;
using Dapper.CQRS;
using NExpect;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.DatabaseServices;
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
                var user = TestUser.Create();
                var commandExecutor = Resolve<IAccountCommands>();
                var queryExecutor = Resolve<IAccountQueries>();
                // act
                var userAccount = commandExecutor.InsertUser(user);
                var updatedUser = TestUser.Create();
                updatedUser.RefreshToken = null;
                updatedUser.Id = userAccount.Id;
                
                commandExecutor.UpdateUser(updatedUser);
                var expected = queryExecutor.FetchUserById<TestUser>(id);
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
                // assert
                Expect(updatedUser).To.Deep.Equal(expected);
            }
        }
    }
}