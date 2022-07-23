using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ntk8.Constants;
using Ntk8.DatabaseServices;
using Ntk8.Exceptions;
using Ntk8.Models;
using Ntk8.Services;
using Ntk8.Utilities;

namespace Ntk8.Middleware
{
    public class JwtMiddleware<T> : IMiddleware where T : class, IBaseUser, new()
    {
        private readonly INtk8Queries<T> _ntk8Queries;
        private readonly IAuthSettings _authSettings;
        private readonly ITokenService _tokenService;
        private readonly IGlobalSettings _globalSettings;

        public JwtMiddleware(
            INtk8Queries<T> ntk8Queries,
            IAuthSettings authSettings,
            ITokenService tokenService,
            IGlobalSettings globalSettings)
        {
            _ntk8Queries = ntk8Queries;
            _authSettings = authSettings;
            _tokenService = tokenService;
            _globalSettings = globalSettings;
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

                var useJwt = _globalSettings.UseJwt;

                if (token is not null)
                {
                    MountUserToContext(context, token, useJwt);
                }

                var refreshToken = context.GetRefreshToken();

                if (token is null 
                    && refreshToken is not null)
                {
                    MountUserToContext(context, refreshToken, useJwt);
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

        public void MountUserToContext(HttpContext httpContext, string token, bool useJwt)
        {
            IBaseUser? user;
            switch (useJwt)
            {
                case true:
                {
                    var validatedToken = _tokenService
                        .ValidateJwtSecurityToken(token, _authSettings.RefreshTokenSecret ?? string.Empty);
                
                    if (validatedToken is null)
                    {
                        throw new InvalidJwtTokenException();
                    }
            
                    var jwtToken = (JwtSecurityToken) validatedToken;

                    var accountId = int.Parse(jwtToken.Claims
                        .First(x => x.Type == AuthenticationConstants.PrimaryKeyValue)
                        .Value ?? string.Empty);

                    user = _ntk8Queries.FetchUserById(accountId);

                    if (user is null)
                    {
                        throw new UserNotFoundException();
                    }
                
                    if (user.RefreshToken?.Token is null 
                        || !user.RefreshToken.IsActive)
                    {
                        throw new InvalidRefreshTokenException();
                    }
                    
                    
                    httpContext.Items.Add(AuthenticationConstants.CurrentUser, user);
                    break;
                }
                case false:
                {
                    user = _ntk8Queries.FetchUserByRefreshToken(token);
                
                    if (user?.RefreshToken is null)
                    {
                        throw new InvalidRefreshTokenException();
                    }
                
                    httpContext.Items.Add(AuthenticationConstants.CurrentUser, user);
                    break;
                }
            }
        }
    }
}