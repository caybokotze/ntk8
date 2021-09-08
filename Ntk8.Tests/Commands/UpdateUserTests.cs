using Dapper.CQRS;
using NExpect;
using NSubstitute;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Models;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Commands
{
    [TestFixture]
    public class UpdateUserTests : TestBase
    {
        [Test]
        public void UpdateUserShouldUpdateUser()
        {
            // arrange
            var user = GetRandom<BaseUser>();
            var commandExecutor = Resolve<ICommandExecutor>();
            var queryExecutor = Resolve<IQueryExecutor>();
            // act
            using (Transactions.UncommittedRead())
            {
                var id = commandExecutor.Execute(new InsertUser(user));
                var updatedUser = GetRandom<BaseUser>();
                updatedUser.Id = id;
                commandExecutor.Execute(new UpdateUser(updatedUser));
                var expected = queryExecutor.Execute(new FetchUserById(id));
                updatedUser.DateModified = expected.DateModified;
                updatedUser.DateCreated = expected.DateCreated;
                updatedUser.VerificationDate = expected.VerificationDate;
                updatedUser.PasswordResetDate = expected.PasswordResetDate;
                updatedUser.ResetTokenExpires = expected.ResetTokenExpires;
                updatedUser.RefreshTokens = null;
                updatedUser.UserRoles = null;
                // assert
                Expect(updatedUser).To.Deep.Equal(expected);
            }
        }
    }
}