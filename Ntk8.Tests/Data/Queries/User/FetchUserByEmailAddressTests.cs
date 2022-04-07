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
    public class FetchUserByEmailAddressTests
    {
        [TestFixture]
        public class Behaviour : TestFixtureWithServiceProvider
        {
            [Test]
            public void ShouldFetchAllUserProperties()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var queryExecutor = Resolve<IQueryExecutor>();
                    var commandExecutor = Resolve<ICommandExecutor>();
                    var user = GetRandom<IBaseUser>();
                    // act
                    var userId = commandExecutor.Execute(new InsertUser(user));
                    var expectedUser = queryExecutor.Execute(new FetchUserByEmailAddress<TestUser>(user.Email));
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
                    user.Roles = null;
                    user.RefreshToken = null;
                    user.Roles = null;
                    expectedUser.Roles = null;
                    expectedUser.RefreshToken = null;
                    expectedUser.Roles = null;
                    Expect(user)
                        .To.Deep.Equal(expectedUser);
                }
            }

            [Test]
            public void ShouldAttachUserRolesToFirstResult()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var commandExecutor = Resolve<ICommandExecutor>();
                    var queryExecutor = Resolve<IQueryExecutor>();
                    var user = GetRandom<IBaseUser>();
                    var userRoles = GetRandomArray<UserRole>();
                    var roles = new Role[userRoles.Length];
                    var i = 0;
                    foreach (var role in userRoles)
                    {
                        roles[i] = GetRandom<Role>();
                        roles[i].Id = 100 + i;
                        role.UserId = user.Id;
                        role.RoleId = roles[i].Id;
                        i++;
                    }
                    var userId = commandExecutor.Execute(new InsertUser(user));
                    foreach (var role in roles)
                    {
                        commandExecutor.Execute(new InsertRole(role));
                    }
                    foreach (var userRole in userRoles)
                    {
                        userRole.UserId = userId;
                        commandExecutor.Execute(new InsertUserRole(userRole));
                    }

                    // act
                    var result = queryExecutor
                        .Execute(new FetchUserByEmailAddress<TestUser>(user.Email));

                    foreach (var role in roles)
                    {
                        role.UserRoles = null;
                    }
                    
                    // assert
                    Expect(result).Not.To.Be.Null();
                    Expect(result.Roles).To.Deep.Equal(roles);
                }
            }

            [Test]
            public void ShouldAttachRefreshTokenToUser()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var commandExecutor = Resolve<ICommandExecutor>();
                    var queryExecutor = Resolve<IQueryExecutor>();
                    var user = GetRandom<IBaseUser>();
                    var userId = commandExecutor.Execute(new InsertUser(user));

                    var refreshToken = GetRandom<Ntk8.Models.RefreshToken>();
                    refreshToken.UserId = userId;
                    var refreshId = commandExecutor.Execute(new InsertRefreshToken(refreshToken));
                    refreshToken.BaseUser = null;
                    refreshToken.Id = refreshId;
                    // act
                    var result = queryExecutor
                        .Execute(new FetchUserByEmailAddress<TestUser>(user.Email));

                    // assert
                    Expect(result.RefreshToken.Expires).To.Approximately.Equal((DateTime)refreshToken.Expires!);
                    Expect(result.RefreshToken.DateCreated).To.Approximately.Equal(refreshToken.DateCreated);
                    Expect(result.RefreshToken.DateRevoked).To.Approximately.Equal(refreshToken.DateRevoked ?? default);
                    result.RefreshToken.Expires = refreshToken.Expires;
                    result.RefreshToken.DateCreated = refreshToken.DateCreated;
                    result.RefreshToken.DateRevoked = refreshToken.DateRevoked;
                    Expect(result).Not.To.Be.Null();
                    Expect(result.RefreshToken).To.Deep.Equal(refreshToken);
                }
            }

            [Test]
            public void ShouldNotAttachRolesOrRefreshTokensIfNull()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var commandExecutor = Resolve<ICommandExecutor>();
                    var queryExecutor = Resolve<IQueryExecutor>();
                    var user = GetRandom<IBaseUser>();
                   
                    commandExecutor.Execute(new InsertUser(user));

                    // act
                    var result = queryExecutor
                        .Execute(new FetchUserByEmailAddress<TestUser>(user.Email));

                    // assert
                    Expect(result).Not.To.Be.Null();
                    Expect(result.RefreshToken).To.Be.Null();
                    Expect(result.Roles.Length).To.Equal(0);
                }
            }
        }
    }
}