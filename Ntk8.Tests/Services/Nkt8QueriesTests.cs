using Dapper.CQRS;
using NExpect;
using NSubstitute;
using Ntk8.Data.Queries;
using Ntk8.DatabaseServices;
using Ntk8.Tests.TestHelpers;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Services;

[TestFixture]
public class Nkt8QueriesTests
{
    [TestFixture]
    public class FetchUserByIdTests
    {
        [Test]
        public void ShouldCallExpectedQueriesWithExpectedParameters()
        {
            // arrange
            var queryExecutor = Substitute.For<IQueryExecutor>();
            var randomUser = GetRandom<TestUserEntity>();
            queryExecutor.Execute(Arg.Any<FetchUserById<TestUserEntity>>())
                .Returns(randomUser);
            var sut = Substitute.For<UserQueries>(queryExecutor);
            // act
            var result = sut.FetchUserById(3);
            // assert
            Expect(queryExecutor).To.Have.Received(1)
                .Execute(Arg.Is<FetchUserById<TestUserEntity>>(s => s.Id == 3));
            Expect(result).To.Equal(randomUser);
        }
    }

    [TestFixture]
    public class FetchUserByEmailAddressTests
    {
        [Test]
        public void ShouldCallExpectedQueriesWithExpectedParameters()
        {
            // arrange
            var queryExecutor = Substitute.For<IQueryExecutor>();
            var sut = Substitute.For<UserQueries>(queryExecutor);
            // act
            var email = GetRandomEmail();
            sut.FetchUserByEmailAddress(email);
            // assert
            Expect(queryExecutor).To.Have.Received(1)
                .Execute(Arg.Is<FetchUserByEmailAddress<TestUserEntity>>(s => s.EmailAddress == email));
        }
    }

    [TestFixture]
    public class FetchUserByRefreshTokenTests
    {
        [Test]
        public void ShouldCallExpectedQueriesWithExpectedParameters()
        {
            // arrange
            var queryExecutor = Substitute.For<IQueryExecutor>();
            var sut = Substitute.For<UserQueries>(queryExecutor);
            // act
            var refreshToken = GetRandomAlphaString();
            sut.FetchUserByRefreshToken(refreshToken);
            // assert
            Expect(queryExecutor).To.Have.Received(1)
                .Execute(Arg.Is<FetchUserByRefreshToken<TestUserEntity>>(s => s.Token == refreshToken));
        }
    }

    [TestFixture]
    public class FetchUserByResetTokenTests
    {
        [Test]
        public void ShouldCallExpectedQueriesWithExpectedParameters()
        {
            // arrange
            var queryExecutor = Substitute.For<IQueryExecutor>();
            var sut = Substitute.For<UserQueries>(queryExecutor);
            // act
            var resetToken = GetRandomAlphaString();
            sut.FetchUserByResetToken(resetToken);
            // assert
            Expect(queryExecutor).To.Have.Received(1)
                .Execute(Arg.Is<FetchUserByResetToken<TestUserEntity>>(s => s.Token == resetToken));
        }
    }

    [TestFixture]
    public class FetchUserByVerificationTokenTests
    {
        [Test]
        public void ShouldCallExpectedQueriesWithExpectedParameters()
        {
            // arrange
            var queryExecutor = Substitute.For<IQueryExecutor>();
            var sut = Substitute.For<UserQueries>(queryExecutor);
            // act
            var verificationToken = GetRandomAlphaString();
            sut.FetchUserByVerificationToken(verificationToken);
            // assert
            Expect(queryExecutor).To.Have.Received(1)
                .Execute(Arg.Is<FetchUserByVerificationToken<TestUserEntity>>(s => s.Token == verificationToken));
        }
    }
}