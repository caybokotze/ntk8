using Microsoft.AspNetCore.Http;
using Ntk8.Constants;
using Ntk8.Models;

namespace Ntk8.Utilities;

public static class HttpContextHelpers
{
    public static string? GetRemoteIpAddress(this IHttpContextAccessor contextAccessor)
    {
        return contextAccessor
            .HttpContext
            .Connection
            .RemoteIpAddress
            ?.MapToIPv4()
            .ToString();
    }

    public static IBaseUser? GetCurrentUser(this HttpContext httpContext)
    {
        if(httpContext.Items.TryGetValue(AuthenticationConstants.CurrentUser, out var value))
        {
            return (IBaseUser?) value ?? null;
        }

        return null;
    }

    public static IHeaderDictionary GetRequestHeaders(this IHttpContextAccessor contextAccessor)
    {
        return contextAccessor
            .HttpContext
            .Request
            .Headers;
    }

    public static string GetIpAddress(this IHttpContextAccessor contextAccessor)
    {
        if (contextAccessor
            .GetRequestHeaders()
            .TryGetValue(ControllerConstants.IpForwardHeader, out var value))
        {
            return value;
        }

        return contextAccessor.GetRemoteIpAddress() ?? string.Empty;
    }

    public static string? GetRefreshToken(this HttpContext context)
    {
        context
            .Request
            .Cookies.TryGetValue(AuthenticationConstants.RefreshToken, out var value);

        return value;
    }
}