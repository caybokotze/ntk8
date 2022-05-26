using Dapper.CQRS;
using NExpect;
using NSubstitute;
using Ntk8.Data.Queries;
using Ntk8.Tests.DatabaseServices;
using Ntk8.Tests.Helpers;
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
            var sut = Substitute.For<Ntk8Queries<TestUser>>(queryExecutor);
            // act
            sut.FetchUserById(3);
            // assert
            Expect(queryExecutor).To.Have.Received(1)
                .Execute(Arg.Is<FetchUserById<TestUser>>(s => s.Id == 3));
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
            var sut = Substitute.For<Ntk8Queries<TestUser>>(queryExecutor);
            // act
            var email = GetRandomEmail();
            sut.FetchUserByEmailAddress(email);
            // assert
            Expect(queryExecutor).To.Have.Received(1)
                .Execute(Arg.Is<FetchUserByEmailAddress<TestUser>>(s => s.EmailAddress == email));
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
            var sut = Substitute.For<Ntk8Queries<TestUser>>(queryExecutor);
            // act
            var refreshToken = GetRandomAlphaString();
            sut.FetchUserByRefreshToken(refreshToken);
            // assert
            Expect(queryExecutor).To.Have.Received(1)
                .Execute(Arg.Is<FetchUserByRefreshToken<TestUser>>(s => s.Token == refreshToken));
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
            var sut = Substitute.For<Ntk8Queries<TestUser>>(queryExecutor);
            // act
            var resetToken = GetRandomAlphaString();
            sut.FetchUserByResetToken(resetToken);
            // assert
            Expect(queryExecutor).To.Have.Received(1)
                .Execute(Arg.Is<FetchUserByResetToken<TestUser>>(s => s.Token == resetToken));
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
            var sut = Substitute.For<Ntk8Queries<TestUser>>(queryExecutor);
            // act
            var verificationToken = GetRandomAlphaString();
            sut.FetchUserByVerificationToken(verificationToken);
            // assert
            Expect(queryExecutor).To.Have.Received(1)
                .Execute(Arg.Is<FetchUserByVerificationToken<TestUser>>(s => s.Token == verificationToken));
        }
    }
}