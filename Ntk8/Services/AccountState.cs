using System.Linq;
using Microsoft.AspNetCore.Http;
using Ntk8.Constants;
using Ntk8.Models;

namespace Ntk8.Services;

public interface IAccountState
{
    IBaseUser? CurrentUser { get; }
    string? CurrentRefreshToken { get; }
    string? CurrentJwtToken { get; }

    void SetCurrentUser(IBaseUser user);
}

public class AccountState : IAccountState
{
    private readonly IHttpContextAccessor _contextAccessor;

    public AccountState(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public void SetCurrentUser(IBaseUser user)
    {
        CurrentUser = user;
    }

    public string? CurrentJwtToken => _contextAccessor
        .HttpContext
        .Request
        .Headers[AuthenticationConstants.DefaultJwtHeader];

    public IBaseUser? CurrentUser { get; private set; }

    public string? CurrentRefreshToken => _contextAccessor
        .HttpContext
        .Request
        .Cookies[AuthenticationConstants.RefreshToken];
}