using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.CQRS;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Ntk8.Constants;
using Ntk8.Data.Queries;
using Ntk8.Exceptions;
using Ntk8.Models;

namespace Ntk8.Middleware
{
    public class JwtMiddleware : IMiddleware
    {
        private readonly AuthSettings _authSettings;
        private readonly IQueryExecutor _queryExecutor;

        public JwtMiddleware(
            AuthSettings authSettings,
            IQueryExecutor queryExecutor)
        {
            _authSettings = authSettings;
            _queryExecutor = queryExecutor;
        }

        public async Task InvokeAsync(
            HttpContext context, RequestDelegate next)
        {
            var token = context.Request.Headers[AuthenticationConstants.DefaultJwtHeader]
                .FirstOrDefault()
                ?.Split(" ")
                .Last();

            if (token != null)
            {
                context = MountUserToContext(context, token);
            }

            try
            {
                await next.Invoke(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                var bytes = Encoding.UTF8.GetBytes(ex.Message);
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public HttpContext MountUserToContext(
            HttpContext context,
            string token)
        {
            if (_authSettings.RefreshTokenSecret.Length < 32)
            {
                throw new InvalidTokenLengthException();
            }

            if (token is null)
            {
                throw new NullReferenceException("The jwt token is null and cannot be mounted to the context");
            }

            var validatedToken = ValidateJwtSecurityToken(token, _authSettings.RefreshTokenSecret);

            var jwtToken = (JwtSecurityToken) validatedToken;

            var accountId = int.Parse(jwtToken.Claims
                .First(x => x.Type == AuthenticationConstants.PrimaryKeyValue)
                .Value);

            context.Items[AuthenticationConstants.ContextAccount] =
                _queryExecutor.Execute(new FetchUserById(accountId));
            
            return context;
        }

        private SecurityToken ValidateJwtSecurityToken(string jwtToken, string refreshTokenSecret)
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
    }
}