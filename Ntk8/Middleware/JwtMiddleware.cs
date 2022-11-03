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
    public class JwtMiddleware<T> : IMiddleware where T : class, IUserEntity, new()
    {
        private readonly IAccountQueries _accountQueries;
        private readonly IAuthSettings _authSettings;
        private readonly ITokenService _tokenService;
        private readonly IGlobalSettings _globalSettings;

        public JwtMiddleware(
            IAccountQueries accountQueries,
            IAuthSettings authSettings,
            ITokenService tokenService,
            IGlobalSettings globalSettings)
        {
            _accountQueries = accountQueries;
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
                var token = context.GetJwtToken();

                var useJwt = _globalSettings.UseJwt;

                if (token is not null && useJwt)
                {
                    MountUserToContext(context, token, useJwt);
                }

                var refreshToken = context.GetRefreshToken();

                if (refreshToken is not null
                    && !useJwt)
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
            IUserEntity? user;
            switch (useJwt)
            {
                case true:
                {
                    var validatedToken = _tokenService
                        .ValidateJwtSecurityToken(token, _authSettings.RefreshTokenSecret ?? string.Empty);

                    if (validatedToken is not JwtSecurityToken jwtToken)
                    {
                        throw new InvalidJwtTokenException();
                    }

                    var accountId = int.Parse(jwtToken.Claims
                        .First(x => x.Type == AuthenticationConstants.PrimaryKeyValue)
                        .Value ?? string.Empty);

                    user = _accountQueries.FetchUserById<T>(accountId);

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
                    user = _accountQueries.FetchUserByRefreshToken<T>(token);
                
                    if (user?.RefreshToken?.IsActive ?? false)
                    {
                        httpContext.Items.Add(AuthenticationConstants.CurrentUser, user);
                    }
                
                    break;
                }
            }
        }
    }
}