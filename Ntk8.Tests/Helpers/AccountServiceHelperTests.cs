using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NExpect;
using Ntk8.Exceptions;
using Ntk8.Helpers;
using Ntk8.Models;
using NUnit.Framework;
using PeanutButter.DuckTyping;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Helpers
{
    [TestFixture]
    public class AccountServiceHelperTests
    {
        [TestFixture]
        public class GenerateJwtToken
        {
            [Test]
            public void ShouldGenerateTokenThatIsNotNull()
            {
                // arrange
                var authSettings = new AuthSettings
                {
                    RefreshTokenSecret = GetRandomAlphaString(34)
                };
                
                var token = CreateJwtToken(authSettings);
                // act
                // assert
                Expect(token)
                    .Not
                    .To
                    .Be
                    .Null();
            }

            [Test]
            [Repeat(5)]
            public void ShouldGenerateValidJwtToken()
            {
                // arrange
                var authSettings = new AuthSettings
                {
                    RefreshTokenSecret = GetRandomString(35)
                };

                var user = TestUser.Create();
                user.Roles = new Role[]
                {
                    new Role
                    {
                        Id = 1,
                        RoleName = "Administrator"
                    },
                    new Role
                    {
                        Id = 2,
                        RoleName = "Manager"
                    }
                };

                // act
                var token = CreateJwtToken(authSettings, user);
                
                var header = token
                    .Split(".")[0];

                var payload = token
                    .Split(".")[1];
                
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                
                // assert

                Expect(header).To.Not.Be.Empty();
                Expect(payload).To.Not.Be.Empty();
                Expect(header).To.Be.Equal.To(jwtToken.Header.Base64UrlEncode());
                Expect(payload).To.Be.Equal.To(jwtToken.Payload.Base64UrlEncode());
                Expect(jwtToken.Claims.First(f => f.Type == "id").Value)
                    .To.Be.Equal.To(user.Id.ToString());
                Expect(jwtToken.IssuedAt).To.Approximately.Equal(DateTime.UtcNow);
                Expect(jwtToken.ValidFrom).To.Approximately.Equal(DateTime.UtcNow);
                Expect(jwtToken.ValidTo).To.Approximately.Equal(DateTime.UtcNow.AddMinutes(15));
                Expect(jwtToken.Issuer).To.Be.Equal.To("NTK8");
                Expect(jwtToken.SignatureAlgorithm).To.Equal("HS256");
                Expect(jwtToken.Claims.First(f => f.Type == "roles").Value).To
                    .Equal(JsonConvert.SerializeObject(user.Roles));
            }

            [TestFixture]
            public class WhenAuthSettingSecretIsTooShort
            {
                [Test]
                public void ShouldThrow()
                {
                    // arrange
                    var authSettings = new AuthSettings()
                    {
                        RefreshTokenSecret = GetRandomString(8, 29)
                    };
                    // act
                    // assert
                    Expect(() => CreateJwtToken(authSettings))
                        .To
                        .Throw<InvalidTokenLengthException>();
                }
            }
        }

        public static string CreateJwtToken(
            AuthSettings authSettings = null, 
            BaseUser baseUser = null)
        {
            return AccountServiceHelpers.GenerateJwtToken(
                authSettings ??= GetRandom<AuthSettings>(),
                baseUser ??= GetRandom<BaseUser>());
        }
    }
}