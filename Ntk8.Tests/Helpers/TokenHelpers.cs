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
                                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                                SecurityAlgorithms.HmacSha256Signature)
                        });
                    return tokenHandler.WriteToken(token);
                }
    }
}