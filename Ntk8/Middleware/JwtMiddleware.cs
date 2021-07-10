using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DapperDoodle;
using Dispatch.BLL.Data.Queries.UserRelated;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Dispatch.Shared.Configuration;
using Dispatch.Shared.Exceptions;

namespace Dispatch.K8.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;

        public JwtMiddleware(
            RequestDelegate next,
            IOptions<AppSettings> appSettings,
            IQueryExecutor queryExecutor)
        {
            _next = next;
            _appSettings = appSettings.Value;
        }

        public async Task Invoke(
            HttpContext context,
            IQueryExecutor queryExecutor)
        {
            var token = context.Request.Headers[AuthenticationConstants.DefaultJwtHeader]
                .FirstOrDefault()
                ?.Split(" ")
                .Last();

            if (token != null)
            {
                MountAccountToContext(context, queryExecutor, token);
            }
            await _next(context);
        }

        public void MountAccountToContext(
            HttpContext context,
            IQueryExecutor queryExecutor,
            string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken) validatedToken;
                var accountId = int.Parse(jwtToken.Claims
                    .First(x => x.Type == AuthenticationConstants.PrimaryKeyValue)
                    .Value);
                context.Items[AuthenticationConstants.ContextAccount] = 
                    queryExecutor.Execute(new FetchUserById(accountId));
            }
            catch
            {
                throw new ContextNotBoundException();
            }
        }
    }
}