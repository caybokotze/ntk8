using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Ntk8.Constants;
using Ntk8.DatabaseServices;
using Ntk8.Dto;
using Ntk8.Exceptions;
using Ntk8.Models;
using static Newtonsoft.Json.JsonConvert;

namespace Ntk8.Services
{
    public interface ITokenService
    {
        ResetTokenResponse GenerateJwtToken(long userId, Role[]? roles);
        (bool isActive, long userId, Role[]? roles) IsRefreshTokenActive(string token);
        RefreshToken GenerateRefreshToken();
        ResetTokenResponse GenerateJwtToken(string refreshToken);
        void SetRefreshTokenCookie(string token);
        string RandomTokenString();
        SecurityToken ValidateJwtSecurityToken(string jwtToken, string refreshTokenSecret);
        void RevokeRefreshToken(RefreshToken token);
        string GetRefreshToken();
    }

    public class TokenService<T> : ITokenService where T : class, IBaseUser, new()
    {
        private readonly INtk8Commands _ntk8Commands;
        private readonly INtk8Queries<T> _ntk8Queries;
        private readonly IAuthSettings _authSettings;
        private readonly IHttpContextAccessor _contextAccessor;

        public TokenService(
            INtk8Commands ntk8Commands,
            INtk8Queries<T> ntk8Queries,
            IAuthSettings authSettings,
            IHttpContextAccessor contextAccessor)
        {
            _ntk8Commands = ntk8Commands;
            _ntk8Queries = ntk8Queries;
            _authSettings = authSettings;
            _contextAccessor = contextAccessor;
        }
        
        public string GetRefreshToken()
        {
            return _contextAccessor
                .HttpContext
                .Request
                .Cookies[AuthenticationConstants.RefreshToken];
        }

        public void RevokeRefreshToken(RefreshToken token)
        {
            token.DateRevoked = DateTime.UtcNow;
            token.RevokedByIp = GetIpAddress();
            _ntk8Commands.UpdateRefreshToken(token);
        }

        public (bool isActive, long userId, Role[]? roles) IsRefreshTokenActive(string token)
        {
            var user = _ntk8Queries.FetchUserByRefreshToken(token);

            if (user is null)
            {
                throw new InvalidRefreshTokenException();
            }

            var refreshToken = user.RefreshToken;

            if (refreshToken is null)
            {
                throw new InvalidRefreshTokenException();
            }

            return (refreshToken.IsActive, user.Id, user.Roles);
        }
        
        public ResetTokenResponse GenerateJwtToken(string refreshToken)
        {
            var (isActive, userId, roles) = IsRefreshTokenActive(refreshToken);

            if (!isActive)
            {
                throw new InvalidRefreshTokenException();
            }
            
            var jwtToken = GenerateJwtToken(userId, roles);
            
            return new ResetTokenResponse
            {
                Token = jwtToken.Token
            };
        }

        public ResetTokenResponse GenerateJwtToken(long userId, Role[]? roles)
        {
            if (_authSettings.RefreshTokenSecret?.Length < 32)
            {
                throw new InvalidTokenLengthException();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_authSettings.RefreshTokenSecret
                                             ?? throw new RefreshTokenNotIncludedException("The refresh token does not seem to exist in your user settings in the appsettings.json file"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new []
                {
                    new Claim(AuthenticationConstants
                        .PrimaryKeyValue, userId.ToString()),
                    new Claim("roles", SerializeObject(roles))
                }),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime
                    .UtcNow
                    .AddSeconds(_authSettings.JwtTTL == 0 
                        ? 900 
                        : _authSettings.JwtTTL),
                Issuer = "NTK8",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return new ResetTokenResponse
            {
                Token = tokenHandler.WriteToken(token),
                ExpiryDate = DateTime.UtcNow.AddSeconds(_authSettings.JwtTTL)
            };
        }

        public RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken
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
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddSeconds(_authSettings.RefreshTokenTTL)
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
            catch (Exception)
            {
                throw new InvalidJwtTokenException();
            }

            return validatedToken;
        }
        
        private string? GetRemoteIpAddress()
        {
            return _contextAccessor
                .HttpContext
                .Connection
                .RemoteIpAddress
                ?.MapToIPv4().ToString();
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