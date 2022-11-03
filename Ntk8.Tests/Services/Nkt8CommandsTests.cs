using Dapper.CQRS;
using NExpect;
using NSubstitute;
using Ntk8.Data.Commands;
using Ntk8.DatabaseServices;
using Ntk8.Models;
using Ntk8.Tests.TestHelpers;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Services;

[TestFixture]
public class Nkt8CommandsTests
{
    [TestFixture]
    public class DeleteRolesForUserByIdTests
    {
        [Test]
        public void CommandShouldReceiveExpectedParameters()
        {
            // arrange
            var commandExecutor = Substitute.For<ICommandExecutor>();
            var sut = Substitute.For<UserCommands>(commandExecutor);
            var randomId = GetRandomInt();
            // act
            sut.DeleteRolesForUserById(randomId);
            // assert
            Expect(commandExecutor).To.Have.Received(1)
                .Execute(Arg.Is<DeleteRolesForUserById>(s => s.UserId == randomId));
        }
    }

    [TestFixture]
    public class DeleteUserByIdTests
    {
        [Test]
        public void CommandShouldReceiveExpectedParameters()
        {
            // arrange
            var commandExecutor = Substitute.For<ICommandExecutor>();
            var sut = Substitute.For<UserCommands>(commandExecutor);
            var randomId = GetRandomInt();
            // act
            sut.DeleteUserById(randomId);
            // assert
            Expect(commandExecutor).To.Have.Received(1)
                .Execute(Arg.Is<DeleteUserById>(s => s.Id == randomId));
        }
    }

    [TestFixture]
    public class InsertRefreshTokenTests
    {
        [Test]
        public void CommandShouldReceiveExpectedParameters()
        {
            // arrange
            var commandExecutor = Substitute.For<ICommandExecutor>();
            var refreshTokenId = GetRandomInt();
            commandExecutor.Execute(Arg.Any<InsertRefreshToken>()).Returns(refreshTokenId);
            var sut = Substitute.For<UserCommands>(commandExecutor);
            var randomRefreshToken = GetRandom<RefreshToken>();
            // act
            var result = sut.InsertRefreshToken(randomRefreshToken);
            // assert
            Expect(commandExecutor).To.Have.Received(1)
                .Execute(Arg.Is<InsertRefreshToken>(s => s.RefreshToken == randomRefreshToken));
            Expect(result).To.Equal(refreshTokenId);
        }
    }

    public class InsertRoleTests
    {
        [Test]
        public void CommandShouldReceiveExpectedParameters()
        {
            // arrange
            var commandExecutor = Substitute.For<ICommandExecutor>();
            var roleId = GetRandomInt();
            commandExecutor.Execute(Arg.Any<InsertRole>()).Returns(roleId);
            var sut = Substitute.For<UserCommands>(commandExecutor);
            var randomRole = GetRandom<Role>();
            // act
            var result = sut.InsertRole(randomRole);
            // assert
            Expect(commandExecutor).To.Have.Received(1)
                .Execute(Arg.Is<InsertRole>(s => s.Role == randomRole));
            Expect(result).To.Equal(roleId);
        }
    }

    public class InsertUserTests
    {
        [Test]
        public void CommandShouldReceiveExpectedParameters()
        {
            // arrange
            var commandExecutor = Substitute.For<ICommandExecutor>();
            var userId = GetRandomInt();
            commandExecutor.Execute(Arg.Any<InsertUser>()).Returns(userId);
            var sut = Substitute.For<UserCommands>(commandExecutor);
            var randomUser = GetRandom<TestUserEntity>();
            // act
            var result = sut.InsertUser(randomUser);
            // assert
            Expect(commandExecutor).To.Have.Received(1)
                .Execute(Arg.Is<InsertUser>(s => s.UserEntity == randomUser));
            Expect(result).To.Equal(userId);
        }
    }

    public class InsertUserRoleTests
    {
        [Test]
        public void CommandShouldReceiveExpectedParameters()
        {
            // arrange
            var commandExecutor = Substitute.For<ICommandExecutor>();
            var roleId = GetRandomInt();
            commandExecutor.Execute(Arg.Any<InsertUserRole>()).Returns(roleId);
            var sut = Substitute.For<UserCommands>(commandExecutor);
            var userRole = GetRandom<UserRole>();
            // act
            var result = sut.InsertUserRole(userRole);
            // assert
            Expect(commandExecutor).To.Have.Received(1)
                .Execute(Arg.Is<InsertUserRole>(s => s.RoleId == userRole.RoleId && s.UserId == userRole.UserId));
            Expect(result).To.Equal(roleId);
        }
    }

    public class UpdateRefreshTokenTests
    {
        [Test]
        public void CommandShouldReceiveExpectedParameters()
        {
            // arrange
            var commandExecutor = Substitute.For<ICommandExecutor>();
            var sut = Substitute.For<UserCommands>(commandExecutor);
            var refreshToken = GetRandom<RefreshToken>();
            // act
            sut.UpdateRefreshToken(refreshToken);
            // assert
            Expect(commandExecutor).To.Have.Received(1)
                .Execute(Arg.Is<UpdateRefreshToken>(s => s.RefreshToken == refreshToken));
        }
    }

    public class UpdateUserTests
    {
        [Test]
        public void CommandShouldReceiveExpectedParameters()
        {
            // arrange
            var commandExecutor = Substitute.For<ICommandExecutor>();
            var sut = Substitute.For<UserCommands>(commandExecutor);
            var user = GetRandom<TestUserEntity>();
            // act
            sut.UpdateUser(user);
            // assert
            Expect(commandExecutor).To.Have.Received(1)
                .Execute(Arg.Is<UpdateUser>(s => s.UserEntity == user));
        }
    }
}