using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper.CQRS;
using Dispatch.K8;
using Microsoft.IdentityModel.Tokens;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Exceptions;
using Ntk8.Model;

namespace Ntk8.Helpers
{
    public class AccountServiceHelpers
    {
        public static User GetAccount(
            IQueryExecutor queryExecutor,
            int id)
        {
            var account = queryExecutor.Execute(new FetchUserById(id));
            if (account == null)
            {
                throw new KeyNotFoundException("Account Not Found");
            }

            return account;
        }

        public static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string GenerateJwtToken(User userModel)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(RandomString(28));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new []
                {
                    new Claim(AuthenticationConstants
                        .PrimaryKeyValue, userModel.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static (RefreshToken, User) GetRefreshToken(IQueryExecutor queryExecutor, string token)
        {
            // todo: This only looks for the refresh token attached to the user currently.
            
            var account = queryExecutor.Execute(new FetchUserByResetToken(token));
            if (account == null)
            {
                throw new InvalidTokenException(AuthenticationConstants.InvalidAuthenticationMessage);
            }

            var refreshToken = account.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive)
            {
                throw new InvalidTokenException(AuthenticationConstants.InvalidAuthenticationMessage);
            }

            return (refreshToken, account);
        }

        public static void RemoveOldRefreshTokens(AppSettings appSettings, UserModel userModel)
        {
            userModel.RefreshTokens.RemoveAll(x => !x.IsActive &&
                                              x.Created.AddDays(appSettings.RefreshTokenTTL)
                                              <= DateTime.UtcNow);
        }

        public static RefreshToken GenerateRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = RandomTokenString(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
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