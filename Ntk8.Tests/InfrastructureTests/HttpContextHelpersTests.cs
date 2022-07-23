using Microsoft.AspNetCore.Http;
using NExpect;
using Ntk8.Utilities;
using NUnit.Framework;

namespace Ntk8.Tests.InfrastructureTests;

public class HttpContextHelpersTests
{
    [TestFixture]
    public class GetRefreshToken
    {
        [Test]
        public void ShouldGetRefreshTokenAsExpected()
        {
            // arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Cookie"] = "refreshToken=randomvalue";
            // act
            var result = httpContext.GetRefreshToken();
            // assert
            Expectations.Expect(result).To.Equal("randomvalue");
        }

        [TestFixture]
        public class WhenTokenDoesNotExist
        {
            [Test]
            public void ShouldReturnNull()
            {
                // arrange
                var httpContext = new DefaultHttpContext();
                // act
                var result = httpContext.GetRefreshToken();
                // assert
                Expectations.Expect(result).To.Be.Null();
            }
        }
    }
}