using System;
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
using static Newtonsoft.Json.JsonConvert;

namespace Ntk8.Services
{
    public interface ITokenService
    {
        BaseUser GetAccount(int id);
        string GenerateJwtToken(BaseUser baseUserModel);
        BaseUser FetchUserAndCheckIfRefreshTokenIsActive(string token);

        BaseUser RemoveOldRefreshTokens(
            BaseUser baseUserModel);

        RefreshToken GenerateRefreshToken(string ipAddress);
    }

    public class TokenService : ITokenService
    {
        private readonly IQueryExecutor _queryExecutor;
        private readonly AuthSettings _authSettings;

        public TokenService(
            IQueryExecutor queryExecutor,
            AuthSettings authSettings)
        {
            _queryExecutor = queryExecutor;
            _authSettings = authSettings;
        }
        
        public BaseUser GetAccount(int id)
        {
            var account = _queryExecutor.Execute(new FetchUserById(id));
            if (account == null)
            {
                throw new UserNotFoundException("The user can not be found.");
            }

            return account;
        }

        public string GenerateJwtToken(BaseUser baseUserModel)
        {
            if (_authSettings.RefreshTokenSecret.Length < 32)
            {
                throw new InvalidTokenLengthException();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_authSettings.RefreshTokenSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new []
                {
                    new Claim(AuthenticationConstants
                        .PrimaryKeyValue, baseUserModel.Id.ToString()),
                    new Claim("roles", SerializeObject(baseUserModel.Roles))
                }),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime
                    .UtcNow
                    .AddSeconds(_authSettings.JwtTTL == 0 ? 900 : _authSettings.JwtTTL),
                Issuer = "NTK8",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public BaseUser FetchUserAndCheckIfRefreshTokenIsActive(string token)
        {
            var account = _queryExecutor
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

        public BaseUser RemoveOldRefreshTokens(
            BaseUser baseUserModel)
        {
            baseUserModel
                .RefreshTokens
                .ToList()
                .RemoveAll(x => !x.IsActive &&
                                x.DateCreated.AddSeconds(_authSettings.RefreshTokenTTL)
                                <= DateTime.UtcNow);

            return baseUserModel;
        }

        public RefreshToken GenerateRefreshToken(string ipAddress)
        {
            return new()
            {
                Token = RandomTokenString(),
                Expires = DateTime.UtcNow.AddSeconds(_authSettings.RefreshTokenTTL),
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