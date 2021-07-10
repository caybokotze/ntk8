using System;
using Dispatch.K8;
using Microsoft.AspNetCore.Http;
using Ntk8.Model;

namespace Ntk8.Services
{
    public class AuthenticationContextService
    {
        public HttpContext HttpContext { get; }

        public AuthenticationContextService(IHttpContextAccessor httpContextAccessor)
        {
            HttpContext = httpContextAccessor.HttpContext;
        }

        public User User => (User) HttpContext.Items[AuthenticationConstants.ContextAccount];
        
        public string GetRefreshToken()
        {
            return HttpContext.Request.Cookies[AuthenticationConstants.RefreshToken];
        }
        
        public string GetOriginRequestHeader()
        {
            return HttpContext.Request.Headers["origin"];
        }
        
        public IHeaderDictionary GetRequestHeaders()
        {
            return HttpContext.Request.Headers;
        }
        
        public string GetRemoteIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }
        
        public string GetLocalIpAddress()
        {
            return HttpContext.Connection.LocalIpAddress?.MapToIPv4().ToString();
        }
        
        public string GetIpAddress()
        {
            if (GetRequestHeaders().ContainsKey(ControllerConstants.IpForwardHeader))
            {
                return GetRequestHeaders()[ControllerConstants.IpForwardHeader];
            }

            return GetRemoteIpAddress();
        }
        
        public void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            HttpContext.Response.Cookies.Append(
                AuthenticationConstants.RefreshToken, 
                token,
                cookieOptions);
        }
        
    }
}