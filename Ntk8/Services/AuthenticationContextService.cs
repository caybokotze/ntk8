using System;
using Microsoft.AspNetCore.Http;
using Ntk8.Constants;
using Ntk8.Models;

namespace Ntk8.Services
{
    public interface IAuthenticationContextService
    {
        HttpContext HttpContext { get; }
        BaseUser BaseUser { get; }
        string GetRefreshToken();
        string GetOriginRequestHeader();
        IHeaderDictionary GetRequestHeaders();
        string GetRemoteIpAddress();
        string GetLocalIpAddress();
        string GetIpAddress();
        void SetTokenCookie(string token);
    }

    public class AuthenticationContextService : IAuthenticationContextService
    {
        private readonly AuthSettings _authSettings;
        public HttpContext HttpContext { get; }

        public AuthenticationContextService(
            IHttpContextAccessor httpContextAccessor,
            AuthSettings authSettings)
        {
            _authSettings = authSettings;
            HttpContext = httpContextAccessor.HttpContext;
        }

        public BaseUser BaseUser => (BaseUser) HttpContext.Items[AuthenticationConstants.ContextAccount];
        
        
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