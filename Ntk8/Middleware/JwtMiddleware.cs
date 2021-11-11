using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.CQRS;
using Microsoft.AspNetCore.Http;
using Ntk8.Constants;
using Ntk8.Data.Queries;
using Ntk8.Exceptions;
using Ntk8.Models;
using Ntk8.Services;

namespace Ntk8.Middleware
{
    public class JwtMiddleware : IMiddleware
    {
        private readonly IAuthSettings _authSettings;
        private readonly IQueryExecutor _queryExecutor;
        private readonly ITokenService _tokenService;

        public JwtMiddleware(
            IAuthSettings authSettings, 
            IQueryExecutor queryExecutor, 
            ITokenService tokenService)
        {
            _authSettings = authSettings;
            _queryExecutor = queryExecutor;
            _tokenService = tokenService;
        }

        public async Task InvokeAsync(
            HttpContext context, RequestDelegate next)
        {
            var token = context
                .Request
                .Headers[AuthenticationConstants.DefaultJwtHeader]
                .FirstOrDefault()
                ?.Split(" ")
                .Last();
            
            if (token is not null)
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

            var validatedToken = _tokenService
                .ValidateJwtSecurityToken(token, _authSettings.RefreshTokenSecret);

            var jwtToken = (JwtSecurityToken) validatedToken;

            var accountId = int.Parse(jwtToken.Claims
                .First(x => x.Type == AuthenticationConstants.PrimaryKeyValue)
                .Value);

            context.Items[AuthenticationConstants.ContextAccount] =
                _queryExecutor
                    .Execute(new FetchUserById(accountId));
            
            // todo: Role management, attach user roles to the context.
            
            return context;
        }
    }
}