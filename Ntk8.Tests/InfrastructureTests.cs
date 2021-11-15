using Microsoft.Extensions.Configuration;
using NExpect;
using Ntk8.Models;
using NUnit.Framework;
using static NExpect.Expectations;

namespace Ntk8.Tests
{
    [TestFixture]
    public class InfrastructureTests
    {
        [TestFixture]
        public class AuthSettingsTests
        {
            [Test]
            public void ShouldResolveForAllAppSettings()
            {
                // arrange
                var settings = AppSettingProvider.CreateConfigurationRoot();
                var authSettings = settings.GetSection("AuthSettings")
                    .Get<AuthSettings>();
                // act
                // assert
                Expect(authSettings.JwtTTL).To.Be.Greater.Than(0);
                Expect(authSettings.RefreshTokenSecret).To.Not.Be.Null();
                Expect(authSettings.RefreshTokenTTL).To.Be.Greater.Than(0);
                Expect(authSettings.PasswordResetTokenTTL).To.Be.Greater.Than(0);
                Expect(authSettings.UserVerificationTokenTTL).To.Be.Greater.Than(0);
            }
        }
    }
}