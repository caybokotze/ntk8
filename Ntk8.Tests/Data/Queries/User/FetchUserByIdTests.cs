using System;
using System.Transactions;
using Dapper.CQRS;
using NExpect;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Models;
using Ntk8.Tests.Helpers;
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
                    var expectedUser = TestUser.Create();
                    expectedUser.RefreshToken = null;
                    var userId = commandExecutor.Execute(new InsertUser(expectedUser));
                    var actualUser = queryExecutor.Execute(new FetchUserById<TestUser>(userId));
                    expectedUser.Id = userId;
                    expectedUser.Roles = null;
                    actualUser!.Roles = null;
                    actualUser!.RefreshToken = null;
                    actualUser.UserRoles = null;
                    // assert
                    Expect(actualUser.DateCreated)
                        .To.Approximately.Equal(expectedUser.DateCreated);
                    Expect(actualUser.DateModified)
                        .To.Approximately.Equal(expectedUser.DateModified);
                    Expect(actualUser.DateVerified)
                        .To.Approximately.Equal(expectedUser.DateVerified ?? default);
                    Expect(actualUser.DateOfPasswordReset)
                        .To.Approximately.Equal(expectedUser.DateOfPasswordReset ?? default);
                    Expect(actualUser.DateResetTokenExpires)
                        .To.Approximately.Equal(expectedUser.DateResetTokenExpires ?? default);
                    expectedUser.UserRoles = null;
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
                    var user = GetRandom<IBaseUser>();
                    var refreshToken = GetRandom<RefreshToken>();
                    refreshToken.BaseUser = null;
                    // act
                    var userId = commandExecutor
                        .Execute(new InsertUser(user));
                    refreshToken.UserId = userId;
                    var refreshTokenId = commandExecutor
                        .Execute(new InsertRefreshToken(refreshToken));
                    var expectedUser = queryExecutor
                        .Execute(new FetchUserById<TestUser>(userId));
                    refreshToken.Id = refreshTokenId;
                    // assert
                    var expectedToken = expectedUser!.RefreshToken;
                    Expect(expectedToken?.Expires)
                        .To.Approximately.Equal((DateTime)refreshToken.Expires!);
                    Expect(expectedToken?.DateCreated)
                        .To.Approximately.Equal(refreshToken.DateCreated);
                    Expect(expectedToken?.DateRevoked)
                        .To.Approximately.Equal(refreshToken.DateRevoked ?? default);
                    expectedToken!.Expires = refreshToken.Expires;
                    expectedToken.DateCreated = refreshToken.DateCreated;
                    expectedToken.DateRevoked = refreshToken.DateRevoked;
                    Expect(expectedToken)
                        .To.Deep.Equal(refreshToken);
                }
            }
        }

        [TestFixture]
        public class WhenUserHasRoles : TestFixtureWithServiceProvider
        {
            [Test]
            public void ShouldAddSingleRole()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var commandExecutor = Resolve<ICommandExecutor>();
                    var queryExecutor = Resolve<IQueryExecutor>();
                    var user = GetRandom<IBaseUser>();
                    var refreshToken = GetRandom<Ntk8.Models.RefreshToken>();
                    refreshToken.BaseUser = null;
                    // act
                    var userId = commandExecutor
                        .Execute(new InsertUser(user));
                    refreshToken.UserId = userId;
                    var role = new Role
                    {
                        Id = 100,
                        RoleName = GetRandomString()
                    };
                    commandExecutor.Execute(
                        new InsertRole(role));
                    commandExecutor
                        .Execute(new InsertUserRole(new UserRole
                        {
                            RoleId = role.Id,
                            UserId = userId
                        }));
                    var expectedUser = queryExecutor
                        .Execute(new FetchUserById<TestUser>(userId));
                    // assert
                    Expect(expectedUser?.Roles)
                        .To.Contain.Exactly(1)
                        .Deep.Equal.To(role);
                }
            }
            
            [Test]
            public void ShouldAddMultipleRoles()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var commandExecutor = Resolve<ICommandExecutor>();
                    var queryExecutor = Resolve<IQueryExecutor>();
                    var user = GetRandom<IBaseUser>();
                    var refreshToken = GetRandom<Ntk8.Models.RefreshToken>();
                    refreshToken.BaseUser = null;
                    // act
                    var userId = commandExecutor
                        .Execute(new InsertUser(user));
                    refreshToken.UserId = userId;
                    var roles = new Role[]
                    {
                        new()
                        {
                            Id = 100,
                            RoleName = GetRandomString()
                        },
                        new()
                        {
                            Id = 101,
                            RoleName = GetRandomString()
                        }
                    };
                    
                    foreach (var role in roles)
                    {
                        commandExecutor.Execute(
                            new InsertRole(role));
                        
                        commandExecutor
                            .Execute(new InsertUserRole(new UserRole
                            {
                                RoleId = role.Id,
                                UserId = userId
                            }));
                    }
                    
                    var expectedUser = queryExecutor
                        .Execute(new FetchUserById<TestUser>(userId));
                    // assert
                    Expect(expectedUser?.Roles)
                        .To.Deep.Equal(roles);
                }
            }
        }

        [TestFixture]
        public class WhenUserDoesNotHaveRolesOrRefreshTokens : TestFixtureWithServiceProvider
        {
            [Test]
            public void ShouldNotContainAListOfNulls()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var commandExecutor = Resolve<ICommandExecutor>();
                    var queryExecutor = Resolve<IQueryExecutor>();
                    var user = GetRandom<IBaseUser>();
                    // act
                    var userId = commandExecutor
                        .Execute(new InsertUser(user));
                    var expectedUser = queryExecutor
                        .Execute(new FetchUserById<TestUser>(userId));
                    // assert
                    Expect(expectedUser?.Roles?.Length).To.Equal(0);
                    Expect(expectedUser?.RefreshToken).To.Be.Null();
                }
            }
        }
    }
}