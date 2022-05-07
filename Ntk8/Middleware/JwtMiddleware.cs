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
    public class JwtMiddleware<T> : IMiddleware where T : class, IBaseUser, new()
    {
        private readonly IAuthSettings? _authSettings;
        private readonly IQueryExecutor? _queryExecutor;
        private readonly ITokenService? _tokenService;

        public JwtMiddleware(IAuthSettings authSettings, IQueryExecutor queryExecutor, ITokenService tokenService)
        {
            _authSettings = authSettings;
            _tokenService = tokenService;
            _queryExecutor = queryExecutor;
        }

        public async Task InvokeAsync(
            HttpContext context, 
            RequestDelegate next)
        {

            try
            {
                var token = context
                    .Request
                    .Headers[AuthenticationConstants.DefaultJwtHeader]
                    .FirstOrDefault()
                    ?.Split(" ")
                    .Last();

                if (token is not null)
                {
                    MountUserToContext(context, token);
                }

                if (!context.Response.HasStarted)
                {
                    await next.Invoke(context);
                }
            }
            catch (InvalidJwtTokenException ex)
            {
                if (!context.Response.HasStarted)
                {
                    await MessageGenerator(context, ex, 401);
                }
            }
            catch (InvalidRefreshTokenException ex)
            {
                if (!context.Response.HasStarted)
                {
                    await MessageGenerator(context, ex, 401);
                }
            }
        }

        private static async Task MessageGenerator(HttpContext context, Exception ex, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            var bytes = Encoding.UTF8.GetBytes(ex.Message);
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }

        public void MountUserToContext(
            HttpContext context,
            string token)
        {
            var validatedToken = _tokenService
                ?.ValidateJwtSecurityToken(token, _authSettings?.RefreshTokenSecret ?? string.Empty);

            if (validatedToken == null)
            {
                throw new InvalidJwtTokenException();
            }
            
            var jwtToken = (JwtSecurityToken) validatedToken;

            var accountId = int.Parse(jwtToken.Claims
                .First(x => x.Type == AuthenticationConstants.PrimaryKeyValue)
                .Value ?? string.Empty);

            var user = _queryExecutor?.Execute(new FetchUserById<T>(accountId));

            if (string.IsNullOrEmpty(user?.RefreshToken?.Token))
            {
                throw new InvalidRefreshTokenException();
            }

            if (user.RefreshToken.IsExpired || !user.RefreshToken.IsActive)
            {
                throw new InvalidRefreshTokenException();
            }

            context.Items[AuthenticationConstants.ContextAccount] = user;
        }
    }
}