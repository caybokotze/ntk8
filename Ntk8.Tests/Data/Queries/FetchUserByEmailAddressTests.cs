using System;
using System.Collections.Generic;
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
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Data.Queries
{
    [TestFixture]
    public class FetchUserByEmailAddressTests
    {
        [TestFixture]
        public class Behaviour : TestFixtureRequiringServiceProvider
        {
            [Test]
            public void ShouldFetchAllUserProperties()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var queryExecutor = Resolve<IQueryExecutor>();
                    var commandExecutor = Resolve<ICommandExecutor>();
                    var user = TestUserEntity.Create();
                    // act
                    var userId = commandExecutor.Execute(new InsertUser(user));
                    var expectedUser = queryExecutor.Execute(new FetchUserByEmailAddress<TestUserEntity>(user.Email ?? string.Empty));
                    user.Id = userId;
                    // assert
                    Expect(user.DateCreated).To.Approximately.Equal((DateTime)expectedUser?.DateCreated!);
                    Expect(user.DateModified).To.Approximately.Equal(expectedUser.DateModified);
                    Expect(user.DateVerified).To.Approximately
                        .Equal((DateTime)expectedUser.DateVerified!);
                    Expect(user.DateOfPasswordReset).To.Approximately
                        .Equal((DateTime)expectedUser.DateOfPasswordReset!);
                    Expect(user.DateResetTokenExpires).To.Approximately
                        .Equal((DateTime)expectedUser.DateResetTokenExpires!);
                    user.DateCreated = expectedUser.DateCreated;
                    user.DateModified = expectedUser.DateModified;
                    user.DateVerified = expectedUser.DateVerified;
                    user.DateOfPasswordReset = expectedUser.DateOfPasswordReset;
                    user.DateResetTokenExpires = expectedUser.DateResetTokenExpires;
                    user.RefreshToken = null;
                    user.Roles = null;
                    user.UserRoles = null;
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
                    var user = TestUserEntity.Create();
                    var roleId = GetRandomInt(100);
                    var userRoles = new List<UserRole>
                    {
                        new()
                        {
                            Role = new Role
                            {
                                Id = roleId,
                                RoleName = GetRandomString()
                            },
                            RoleId = roleId,
                            UserId = user.Id
                        },
                        new()
                        {
                            Role = new Role
                            {
                                Id = roleId + 1,
                                RoleName = GetRandomString()
                            },
                            RoleId = roleId + 1,
                            UserId = user.Id
                        }
                    };
                    
                    foreach (var role in userRoles)
                    {
                        role.User = null;
                        role.Role!.Id += 100;
                        role.UserId = user.Id;
                        role.RoleId = role.Role.Id += 100;
                    }
                    var userId = commandExecutor.Execute(new InsertUser(user));
                    foreach (var role in userRoles.Select(s => s.Role))
                    {
                        commandExecutor.Execute(new InsertRole(role!));
                    }
                    foreach (var userRole in userRoles)
                    {
                        userRole.UserId = userId;
                        commandExecutor.Execute(new InsertUserRole(userRole));
                    }

                    // act
                    var result = queryExecutor
                        .Execute(new FetchUserByEmailAddress<TestUserEntity>(user.Email ?? string.Empty));

                    // assert
                    Expect(result).Not.To.Be.Null();
                    Expect(result?.Roles).To.Deep.Equal(userRoles.Select(s => s.Role));
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
                    
                    var user = TestUserEntity.Create();
                    var userId = commandExecutor
                        .Execute(new InsertUser(user));

                    var refreshToken = TestUserEntity.CreateValidRefreshToken();
                    refreshToken.UserId = userId;
                    var refreshId = commandExecutor.Execute(new InsertRefreshToken(refreshToken));
                    refreshToken.Id = refreshId;
                    
                    // act
                    var result = queryExecutor
                        .Execute(new FetchUserByEmailAddress<TestUserEntity>(user.Email ?? string.Empty));

                    // assert
                    Expect(result?.RefreshToken?.Expires)
                        .To.Approximately.Equal((DateTime)refreshToken.Expires!);
                    Expect(result?.RefreshToken?.DateCreated)
                        .To.Approximately.Equal(refreshToken.DateCreated);
                    result!.RefreshToken!.Expires = refreshToken.Expires;
                    result.RefreshToken.DateCreated = refreshToken.DateCreated;
                    result.RefreshToken.DateRevoked = refreshToken.DateRevoked;
                    Expect(result.RefreshToken.Token).To.Not.Be.Null();
                    Expect(result.RefreshToken.Token).To.Equal(refreshToken.Token);
                    Expect(result).Not.To.Be.Null();
                    Expect(result.RefreshToken)
                        .To.Deep.Equal(refreshToken);
                }
            }

            [TestFixture]
            public class WhenMultipleRefreshTokensExist : TestFixtureRequiringServiceProvider
            {
                [Test]
                public void ShouldAttachMostRecentRefreshTokenToUser()
                {
                    using (new TransactionScope())
                    {
                        // arrange
                        var commandExecutor = Resolve<ICommandExecutor>();
                        var queryExecutor = Resolve<IQueryExecutor>();
                    
                        var user = TestUserEntity.Create();
                        var userId = commandExecutor
                            .Execute(new InsertUser(user));

                        var refreshToken = GetRandom<RefreshToken>();
                        var refreshToken2 = GetRandom<RefreshToken>();
                        refreshToken.UserId = userId;
                        refreshToken.DateCreated = DateTime.UtcNow;
                        refreshToken2.UserId = userId;
                        refreshToken2.DateCreated = DateTime.UtcNow.AddMinutes(30);
                        var refreshId = commandExecutor.Execute(new InsertRefreshToken(refreshToken));
                        var refreshId2 = commandExecutor.Execute(new InsertRefreshToken(refreshToken2));
                        refreshToken.Id = refreshId;
                        refreshToken2.Id = refreshId2;
                    
                        // act
                        var result = queryExecutor
                            .Execute(new FetchUserByEmailAddress<TestUserEntity>(user.Email ?? string.Empty));

                        // assert
                        Expect(result?.RefreshToken?.DateCreated)
                            .To.Approximately.Equal(refreshToken2.DateCreated);
                        Expect(result?.RefreshToken?.Expires)
                            .To.Approximately.Equal((DateTime)refreshToken2.Expires!);
                        Expect(result?.RefreshToken?.DateRevoked)
                            .To.Approximately.Equal((DateTime)refreshToken2.DateRevoked!);
                        result!.RefreshToken!.Expires = refreshToken2.Expires;
                        result.RefreshToken.DateCreated = refreshToken2.DateCreated;
                        result.RefreshToken.DateRevoked = refreshToken2.DateRevoked;
                        Expect(result).Not.To.Be.Null();
                        Expect(result.RefreshToken)
                            .To.Deep.Equal(refreshToken2);
                    }
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
                    var user = GetRandom<IUserEntity>();
                   
                    commandExecutor.Execute(new InsertUser(user));

                    // act
                    var result = queryExecutor
                        .Execute(new FetchUserByEmailAddress<TestUserEntity>(user.Email ?? string.Empty));

                    // assert
                    Expect(result).Not.To.Be.Null();
                    Expect(result?.RefreshToken).To.Be.Null();
                    Expect(result?.Roles?.Length).To.Equal(0);
                }
            }
        }
    }
}