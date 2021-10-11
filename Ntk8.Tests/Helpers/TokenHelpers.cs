using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Ntk8.Constants;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Helpers
{
    public static class TokenHelpers
    {
        public static string CreateValidJwtToken(string secret = null, int? userId = null)
                {
                    userId ??= GetRandomInt();
                    secret ??= GetRandomString(50);
        
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var token = tokenHandler.CreateToken(
                        new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(new[]
                            {
                                new Claim(AuthenticationConstants.PrimaryKeyValue, userId.ToString())
                            }),
                            Expires = DateTime.UtcNow.AddMinutes(15),
                            SigningCredentials = new SigningCredentials(
                                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                                SecurityAlgorithms.HmacSha256Signature)
                        });
                    return tokenHandler.WriteToken(token);
                }

        public static bool IsJwtTokenValid(string token, string refreshTokenSecret)
        {
            bool isValid;
            var key = Encoding
                .UTF8
                .GetBytes(refreshTokenSecret);
            
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out _);
                isValid = true;
            }
            catch (Exception)
            {
                isValid = false;
            }

            return isValid;
        }
    }
}