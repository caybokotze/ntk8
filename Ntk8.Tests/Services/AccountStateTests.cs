using System.Transactions;
using NExpect;
using Ntk8.Services;
using Ntk8.Tests.TestHelpers;
using NUnit.Framework;
using static NExpect.Expectations;

namespace Ntk8.Tests.Services;

public class AccountStateTests
{
    [TestFixture]
    public class WhenStateHasBeenSet : TestFixtureRequiringServiceProvider
    {
        [Test]
        public void ShouldRetainState()
        {
            using (new TransactionScope())
            {
                // arrange
                var accountState = Resolve<IAccountState>();
                var testUser = TestUser.Create();
                accountState.SetCurrentUser(testUser);
                // act
                var sut = Resolve<IAccountState>();
                // assert
                Expect(accountState.CurrentUser).To.Equal(testUser);
                Expect(sut.CurrentUser).To.Equal(testUser);
            }
        }
    }
}