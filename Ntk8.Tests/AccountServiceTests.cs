using Ntk8.Services;
using NUnit.Framework;

namespace Ntk8.Tests
{
    [TestFixture]
    public class AccountServiceTests
    {
        [TestFixture]
        public class DatabaseTests : TestBase
        {
            [Test]
            public void RegisterDoesRegisterUser()
            {
                // assign
                var accountService = Resolve<IAccountService>();
                // act
                // assert
            }
        }
    }
}