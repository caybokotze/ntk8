using System;
using Microsoft.AspNetCore.Http;
using Ntk8.Constants;
using Ntk8.Models;

namespace Ntk8.Services
{
    public interface IAuthenticationContextService
    {
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthSettings _authSettings;
        

        public AuthenticationContextService(
            IHttpContextAccessor httpContextAccessor,
            IAuthSettings authSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _authSettings = authSettings;
        }

        public BaseUser BaseUser =>
            (BaseUser) _httpContextAccessor
                .HttpContext
                .Items[AuthenticationConstants.ContextAccount];

        public string GetRefreshToken()
        {
            return _httpContextAccessor
                .HttpContext
                .Request
                .Cookies[AuthenticationConstants.RefreshToken];
        }
        
        public string GetOriginRequestHeader()
        {
            return _httpContextAccessor
                .HttpContext
                .Request
                .Headers["origin"];
        }
        
        public IHeaderDictionary GetRequestHeaders()
        {
            return _httpContextAccessor
                .HttpContext
                .Request
                .Headers;
        }
        
        public string GetRemoteIpAddress()
        {
            return _httpContextAccessor
                .HttpContext
                .Connection
                .RemoteIpAddress
                ?.MapToIPv4()
                .ToString();
        }
        
        public string GetLocalIpAddress()
        {
            return _httpContextAccessor
                .HttpContext
                .Connection
                .LocalIpAddress
                ?.MapToIPv4()
                .ToString();
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
            
            _httpContextAccessor.HttpContext.Response.Cookies.Append(
                AuthenticationConstants.RefreshToken, 
                token,
                cookieOptions);
        }
        
    }
}