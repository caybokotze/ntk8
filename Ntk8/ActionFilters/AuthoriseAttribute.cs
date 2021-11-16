using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ntk8.Constants;
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
            var user = (BaseUser) context
                .HttpContext
                .Items[AuthenticationConstants.ContextAccount];

            if (user == null || _roles.Any() && !_roles.Contains(""))
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}