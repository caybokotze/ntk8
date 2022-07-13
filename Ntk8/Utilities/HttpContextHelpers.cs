using Microsoft.AspNetCore.Http;

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
            .TryGetValue(Constants.ControllerConstants.IpForwardHeader, out var value))
        {
            return value;
        }

        return contextAccessor.GetRemoteIpAddress() ?? string.Empty;
    }
}