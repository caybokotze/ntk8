using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Ntk8.Constants;
using Ntk8.Models;
using Ntk8.Services;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Helpers
{
    public static class TokenHelpers
    {
        public static SecurityToken CreateValidJwtToken(string secret = null, long? userId = null)
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
            return token;
        }

        public static string CreateValidJwtTokenAsString(string secret = null, long? userId = null)
        {
            var securityToken = CreateValidJwtToken(secret, userId);

            var tokenHandler = new JwtSecurityTokenHandler();
            
            return tokenHandler
                .WriteToken(securityToken);
        }
        
        public static string CreateValidJwtTokenAsString(SecurityToken securityToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            return tokenHandler
                .WriteToken(securityToken);
        }

        public static RefreshToken CreateRefreshToken()
        {
            var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            var token = BitConverter.ToString(randomBytes).Replace("-", "");
            return new RefreshToken
            {
                Token = token,
                Expires = DateTime.UtcNow.AddSeconds(UserAccountService.RESET_TOKEN_TTL),
                DateCreated = DateTime.UtcNow,
                CreatedByIp = GetRandomIPv4Address()
            };
            
            
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