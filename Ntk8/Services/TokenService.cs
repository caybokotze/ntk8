using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper.CQRS;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Ntk8.Constants;
using Ntk8.Data.Commands;
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
        BaseUser RemoveOldRefreshTokens(BaseUser baseUserModel);
        RefreshToken GenerateRefreshToken();
        void SetRefreshTokenCookie(string token);
        string RandomTokenString();
        SecurityToken ValidateJwtSecurityToken(string jwtToken, string refreshTokenSecret);
        void RevokeRefreshToken(RefreshToken token);
    }

    public class TokenService : ITokenService
    {
        private readonly IQueryExecutor _queryExecutor;
        private readonly ICommandExecutor _commandExecutor;
        private readonly IAuthSettings _authSettings;
        private readonly IHttpContextAccessor _contextAccessor;

        public TokenService(
            IQueryExecutor queryExecutor,
            ICommandExecutor commandExecutor,
            IAuthSettings authSettings,
            IHttpContextAccessor contextAccessor)
        {
            _queryExecutor = queryExecutor;
            _commandExecutor = commandExecutor;
            _authSettings = authSettings;
            _contextAccessor = contextAccessor;
        }
        
        public BaseUser GetAccount(int id)
        {
            var account = _queryExecutor
                .Execute(new FetchUserById(id));
            if (account == null)
            {
                throw new UserNotFoundException("The user can not be found.");
            }

            return account;
        }
        
        public void RevokeRefreshToken(RefreshToken token)
        {
            token.DateRevoked = DateTime.UtcNow;
            token.RevokedByIp = GetIpAddress();
            _commandExecutor.Execute(new UpdateRefreshToken(token));
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
                    new Claim("roles", SerializeObject(
                        baseUserModel
                        .Roles
                        .Select(s => s.RoleName)))
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

        public RefreshToken GenerateRefreshToken()
        {
            return new()
            {
                Token = RandomTokenString(),
                Expires = DateTime.UtcNow.AddSeconds(_authSettings.RefreshTokenTTL),
                DateCreated = DateTime.UtcNow,
                CreatedByIp = GetIpAddress()
            };
        }

        public string RandomTokenString()
        {
            var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }
        
        public void SetRefreshTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            
            _contextAccessor.HttpContext.Response.Cookies.Append(
                AuthenticationConstants.RefreshToken, 
                token,
                cookieOptions);
        }
        
        public SecurityToken ValidateJwtSecurityToken(string jwtToken, string refreshTokenSecret)
        {
            var key = Encoding
                .UTF8
                .GetBytes(refreshTokenSecret);
            
            var tokenHandler = new JwtSecurityTokenHandler();
            
            SecurityToken validatedToken;
            try
            {
                tokenHandler.ValidateToken(jwtToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out validatedToken);
            }
            catch (Exception e)
            {
                throw new InvalidTokenException(e.Message);
            }

            return validatedToken;
        }
        
        private string GetRemoteIpAddress()
        {
            return _contextAccessor
                .HttpContext
                .Connection
                .RemoteIpAddress
                ?.MapToIPv4()
                .ToString();
        }

        private IHeaderDictionary GetRequestHeaders()
        {
            return _contextAccessor
                .HttpContext
                .Request
                .Headers;
        }

        private string GetIpAddress()
        {
            if (GetRequestHeaders()
                .ContainsKey(ControllerConstants.IpForwardHeader))
            {
                return GetRequestHeaders()[ControllerConstants.IpForwardHeader];
            }

            return GetRemoteIpAddress();
        }
    }
}