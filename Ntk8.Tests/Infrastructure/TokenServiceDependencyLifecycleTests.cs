using System.Transactions;
using NExpect;
using Ntk8.Services;
using NUnit.Framework;
using static NExpect.Expectations;

namespace Ntk8.Tests.Infrastructure
{
    [TestFixture]
    public class TokenServiceDependencyLifecycleTests
    {
        [TestFixture]
        public class WhenResolvingForTokenService : TestFixtureWithServiceProvider
        {
            [Test]
            public void ShouldResolveAsTransient()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var tokenService = Resolve<ITokenService>();
                    var tokenService2 = Resolve<ITokenService>();
                    // act
                    // assert
                    Expect(tokenService)
                        .To
                        .Not.Equal(tokenService2);
                }
            }
        }       
    }
}