using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Ntk8.Constants;
using Ntk8.Exceptions;
using Ntk8.Models;

namespace Ntk8.ActionFilters
{
    public class AuthoriseAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;
        
        public AuthoriseAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = (IBaseUser) context
                .HttpContext
                .Items[AuthenticationConstants.ContextAccount];

            if (user == null)
            {
                throw new UserNotAuthenticatedException();
            }

            if (_roles
                .Any(role => (user.Roles ?? Array.Empty<Role>())
                    .All(a => !string.Equals(a.RoleName, role, StringComparison.InvariantCultureIgnoreCase))))
            {
                throw new UserNotAuthorisedException();
            }
        }
    }
}