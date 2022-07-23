using NExpect;
using Ntk8.Models;
using NUnit.Framework;
using static NExpect.Expectations;

namespace Ntk8.Tests.InfrastructureTests;

public class AuthSettingsTests
{
    [Test]
    public void ShouldContainExpectedDefaultValues()
    {
        // arrange
        var authSettings = new AuthSettings();
        // act
        // assert
        Expect(authSettings.RefreshTokenSecret).To.Not.Be.Empty();
        Expect(authSettings.RefreshTokenLength).To.Equal(40);
        Expect(authSettings.JwtTTL).To.Equal(900);
        Expect(authSettings.RefreshTokenTTL).To.Equal(3600);
        Expect(authSettings.PasswordResetTokenTTL).To.Equal(900);
        Expect(authSettings.UserVerificationTokenTTL).To.Equal(900);
    }
}