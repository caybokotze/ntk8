using Microsoft.AspNetCore.Builder;
using Ntk8.Exceptions.Middleware;
using Ntk8.Middleware;
using Ntk8.Models;

namespace Ntk8.Infrastructure
{
    public static class MiddlewareExtensions
    {
        public static void UseNtk8JwtMiddleware<T>(this IApplicationBuilder builder) where T : class, IBaseUser, new()
        {
            builder.UseMiddleware<JwtMiddleware<T>>();
        }

        public static void UseNtk8ExceptionMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<UserNotAuthenticatedExceptionMiddleware>();
            builder.UseMiddleware<UserNotAuthorisedExceptionMiddleware>();
            builder.UseMiddleware<InvalidPasswordExceptionMiddleware>();
            builder.UseMiddleware<UserNotFoundExceptionMiddleware>();
            builder.UseMiddleware<UserAlreadyExistsExceptionMiddleware>();
            builder.UseMiddleware<UserIsNotVerifiedExceptionMiddleware>();
            builder.UseMiddleware<UserIsVerifiedExceptionMiddleware>();
            builder.UseMiddleware<VerificationTokenExpiredExceptionMiddleware>();
            builder.UseMiddleware<InvalidRefreshTokenExceptionMiddleware>();
            builder.UseMiddleware<InvalidJwtTokenExceptionMiddleware>();
        }
    }
}