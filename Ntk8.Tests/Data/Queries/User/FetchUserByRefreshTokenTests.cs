using System.Linq;
using System.Transactions;
using Dapper.CQRS;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using NExpect;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Models;
using Ntk8.Tests.Helpers;
using NUnit.Framework;
using static NExpect.Expectations;

namespace Ntk8.Tests.Data.Queries.User
{
    [TestFixture]
    public class FetchUserByRefreshTokenTests : TestFixtureWithServiceProvider
    {
        [Test]
        public void ShouldFetchUser()
        {
            using (new TransactionScope())
            {
                // arrange
                var queryExecutor = Resolve<IQueryExecutor>();
                var commandExecutor = Resolve<ICommandExecutor>();
                var user = TestUser.Create();
                var refreshToken = user.RefreshTokens.First();
                var userId = commandExecutor.Execute(new InsertUser(user));
                refreshToken.UserId = userId;
                refreshToken.BaseUser = null;
                refreshToken.Id = commandExecutor.Execute(new InsertRefreshToken(refreshToken));
                commandExecutor.Execute(new InsertUserRole(
                    new UserRole
                {
                    UserId = userId,
                    RoleId = (int)Roles.Admin
                }));
                // act
                var retrievedUser = queryExecutor
                    .Execute(new FetchUserByRefreshToken(user.RefreshTokens.First().Token));
                // assert
                Expect(retrievedUser).Not.To.Be.Null();
                Expect(retrievedUser.Roles.First().RoleName).To.Equal(nameof(Roles.Admin));
                Expect(retrievedUser.RefreshTokens.First().DateCreated).To.Approximately
                    .Equal(refreshToken.DateCreated);
                Expect(retrievedUser.RefreshTokens.First().DateRevoked).To.Approximately
                    .Equal(refreshToken.DateRevoked ?? default);
                Expect(retrievedUser.RefreshTokens.First().Expires).To.Approximately.Equal(refreshToken.Expires);
            }
        }
    }
}