using System;
using System.Linq;
using System.Transactions;
using Dapper.CQRS;
using NExpect;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Models;
using Ntk8.Tests.TestHelpers;
using NUnit.Framework;
using static NExpect.Expectations;

namespace Ntk8.Tests.Data.Queries
{
    [TestFixture]
    public class FetchUserByRefreshTokenTests : TestFixtureRequiringServiceProvider
    {
        [Test]
        public void ShouldFetchUser()
        {
            using (new TransactionScope())
            {
                // arrange
                var queryExecutor = Resolve<IQueryExecutor>();
                var commandExecutor = Resolve<ICommandExecutor>();
                var user = TestUserEntity.Create();
                var refreshToken = user.RefreshToken;
                var userId = commandExecutor.Execute(new InsertUser(user));
                refreshToken!.UserId = userId;
                refreshToken.Id = commandExecutor.Execute(new InsertRefreshToken(refreshToken));
                commandExecutor.Execute(new InsertUserRole(
                    new UserRole
                {
                    UserId = userId,
                    RoleId = (int) Roles.Administrator
                }));
                // act
                var expectedUser = queryExecutor
                    .Execute(new FetchUserByRefreshToken<TestUserEntity>(user.RefreshToken?.Token!));
                // assert
                Expect(expectedUser)
                    .Not.To.Be.Null();
                Expect(expectedUser?.Roles?.First()!.RoleName)
                    .To.Equal(nameof(Roles.Administrator));
                Expect(expectedUser?.RefreshToken?.DateCreated)
                    .To.Approximately
                    .Equal(refreshToken.DateCreated);
                Expect(expectedUser?.RefreshToken?.DateRevoked).To.Be.Null();
                Expect(expectedUser?.RefreshToken?.Expires)
                    .To.Approximately.Equal((DateTime)refreshToken.Expires!);
            }
        }
    }
}