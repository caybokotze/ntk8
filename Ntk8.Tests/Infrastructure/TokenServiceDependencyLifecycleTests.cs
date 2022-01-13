using System.Transactions;
using NExpect;
using Ntk8.Services;
using NUnit.Framework;

namespace Ntk8.Tests.Infrastructure
{
    [TestFixture]
    public class TokenServiceDependencyLifecycleTests
    {
        [TestFixture]
        public class WhenResolvingForTokenService : TestFixtureWithServiceProvider
        {
            [Test]
            public void ShouldResolveAsScoped()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var tokenService = Resolve<ITokenService>();
                    var tokenService2 = (ITokenService)HttpContext.RequestServices.GetService(typeof(ITokenService));
                    // act
                    // assert
                    Expectations.Expect(tokenService).To.Equal(tokenService2);
                }
            }
        }       
    }
}