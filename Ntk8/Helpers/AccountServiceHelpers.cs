using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper.CQRS;
using Microsoft.IdentityModel.Tokens;
using Ntk8.Constants;
using Ntk8.Data.Queries;
using Ntk8.Exceptions;
using Ntk8.Models;
using static System.Text.Json.JsonSerializer;

namespace Ntk8.Helpers
{
    public static class AccountServiceHelpers
    {
        public static BaseUser GetAccount(
            IQueryExecutor queryExecutor,
            int id)
        {
            var account = queryExecutor.Execute(new FetchUserById(id));
            if (account == null)
            {
                throw new UserNotFoundException("The user can not be found.");
            }

            return account;
        }

        public static string GenerateJwtToken(
            AuthSettings configuration,
            BaseUser baseUserModel)
        {
            if (configuration.RefreshTokenSecret.Length < 32)
            {
                throw new InvalidTokenLengthException();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration.RefreshTokenSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new []
                {
                    new Claim(AuthenticationConstants
                        .PrimaryKeyValue, baseUserModel.Id.ToString()),
                    new Claim("roles", Serialize(baseUserModel.Roles))
                }),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(15),
                Issuer = "NTK8",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static BaseUser FetchUserAndCheckIfRefreshTokenIsActive(
            IQueryExecutor queryExecutor, 
            string token)
        {
            var account = queryExecutor
                .Execute(new FetchUserByRefreshToken(token));
            
            if (account is null)
            {
                throw new InvalidTokenException(AuthenticationConstants.InvalidAuthenticationMessage);
            }

            RefreshToken refreshToken;
            
            refreshToken = account
                .RefreshTokens
                .Single(x => x.Token == token);


            if (!refreshToken.IsActive)
            {
                throw new InvalidTokenException(AuthenticationConstants.InvalidAuthenticationMessage);
            }

            return account;
        }

        public static BaseUser RemoveOldRefreshTokens(
            AuthSettings configuration,
            BaseUser baseUserModel)
        {
            baseUserModel
                .RefreshTokens
                .RemoveAll(x => !x.IsActive &&
                                x.DateCreated.AddSeconds(configuration.RefreshTokenTTL)
                                <= DateTime.UtcNow);

            return baseUserModel;
        }

        public static RefreshToken GenerateRefreshToken(AuthSettings authSettings, string ipAddress)
        {
            return new()
            {
                Token = RandomTokenString(),
                Expires = DateTime.UtcNow.AddSeconds(authSettings.RefreshTokenTTL),
                DateCreated = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        public static string RandomTokenString()
        {
            var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }
    }
}