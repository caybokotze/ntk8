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
using Ntk8.Models;

namespace Ntk8.Middleware
{
    public class JwtMiddleware : IMiddleware
    {
        private readonly AuthenticationConfiguration _authenticationConfiguration;
        private readonly IQueryExecutor _queryExecutor;

        public JwtMiddleware(
            AuthenticationConfiguration authenticationConfiguration,
            IQueryExecutor queryExecutor)
        {
            _authenticationConfiguration = authenticationConfiguration;
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
                MountAccountToContext(context, token);
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

        public void MountAccountToContext(
            HttpContext context,
            string token)
        {
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII
                .GetBytes(_authenticationConfiguration.RefreshTokenSecret);
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
                _queryExecutor.Execute(new FetchUserById(accountId));
        }
    }
}