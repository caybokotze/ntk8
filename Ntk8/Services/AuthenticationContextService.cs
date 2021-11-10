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
    }

    public class AuthenticationContextService : IAuthenticationContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationContextService(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
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
    }
}